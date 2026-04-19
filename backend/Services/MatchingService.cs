using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Psy.Api.Data;
using Psy.Api.Models;

namespace Psy.Api.Services;

public class MatchingService
{
    private readonly AppDbContext _db;
    private readonly ClaudeClient _claude;
    private readonly ILogger<MatchingService> _log;

    public MatchingService(AppDbContext db, ClaudeClient claude, ILogger<MatchingService> log)
    {
        _db = db;
        _claude = claude;
        _log = log;
    }

    public async Task<ChatResponse> ProcessAsync(ChatRequest req, CancellationToken ct)
    {
        if (!_claude.IsConfigured)
        {
            return new ChatResponse(
                "Le service de chat n'est pas configuré (clé API Anthropic manquante). Contactez l'administrateur.",
                "done", null, null);
        }

        var phase = string.IsNullOrWhiteSpace(req.Phase) ? "initial" : req.Phase;
        var lastUser = req.History.LastOrDefault(m => m.Role == "user")?.Content ?? "";

        return phase switch
        {
            "initial" => await MirrorAsync(req.History, ct),
            "awaiting_confirmation" => await HandleConfirmationAsync(req.History, lastUser, ct),
            "awaiting_followup" => await HandleFollowupAsync(req.History, lastUser, ct),
            _ => new ChatResponse("Merci, bonne journée.", "done", null, null)
        };
    }

    private async Task<ChatResponse> MirrorAsync(List<ChatMessage> history, CancellationToken ct)
    {
        const string system = """
            Tu es l'assistant d'accueil d'une plateforme d'aide psychologique.
            Ton rôle à cette étape est UNIQUEMENT de REFORMULER la problématique de l'utilisateur
            en une phrase claire, sans jargon ni contexte superflu, puis de demander confirmation.
            Style : chaleureux, sobre, en français.
            Format OBLIGATOIRE :
            "Si je comprends bien, vous traversez : <reformulation courte>. Est-ce que j'ai bien saisi ?"
            N'ajoute rien d'autre. Ne donne pas de conseil. Ne propose pas encore de psychologue.
            """;

        var msgs = history.Select(m => (m.Role, m.Content));
        var reply = await _claude.CompleteAsync(system, msgs, 400, ct);
        return new ChatResponse(reply, "awaiting_confirmation", null, null);
    }

    private async Task<ChatResponse> HandleConfirmationAsync(
        List<ChatMessage> history, string lastUser, CancellationToken ct)
    {
        var intent = await ClassifyYesNoAsync(lastUser, ct);

        if (intent == "yes")
            return await MatchAsync(history, ct);

        return await MirrorAsync(history, ct);
    }

    private async Task<ChatResponse> HandleFollowupAsync(
        List<ChatMessage> history, string lastUser, CancellationToken ct)
    {
        var intent = await ClassifyYesNoAsync(lastUser, ct);
        if (intent == "no")
            return new ChatResponse(
                "Très bien, prenez soin de vous. Revenez quand vous le souhaitez.",
                "done", null, null);

        return await MatchAsync(history, ct);
    }

    private async Task<string> ClassifyYesNoAsync(string text, CancellationToken ct)
    {
        const string sys = """
            Classify the user's short reply as one of: "yes", "no", "other".
            Reply with exactly one of those three tokens, lowercase, nothing else.
            """;
        var reply = await _claude.CompleteAsync(sys, new[] { ("user", text) }, 10, ct);
        reply = reply.Trim().ToLowerInvariant();
        if (reply.StartsWith("yes")) return "yes";
        if (reply.StartsWith("no")) return "no";
        return "other";
    }

    private async Task<ChatResponse> MatchAsync(List<ChatMessage> history, CancellationToken ct)
    {
        var psys = await _db.Psychologists
            .Where(p => p.AvailableToday)
            .ToListAsync(ct);

        if (psys.Count == 0)
        {
            return new ChatResponse(
                "Nous n'avons personne de disponible pour le moment. Souhaitez-vous préciser votre besoin pour qu'on puisse chercher plus largement ?",
                "awaiting_followup", null, null);
        }

        var catalog = new StringBuilder();
        foreach (var p in psys)
        {
            catalog.AppendLine($"- id={p.Id} | {p.Name} | {p.Title}");
            catalog.AppendLine($"  skills: {p.SkillsCsv}");
            catalog.AppendLine($"  bio: {p.Bio}");
        }

        var system = $$"""
            Tu es un assistant de matching pour une plateforme de psychologie.
            Voici la liste des psychologues DISPONIBLES AUJOURD'HUI :

            {{catalog}}

            En te basant UNIQUEMENT sur la problématique de l'utilisateur et les bios/compétences
            ci-dessus, détermine s'il y a un bon match.

            Réponds STRICTEMENT en JSON valide, sans texte autour, avec la forme :
            {"match_id": <int ou null>, "reasoning": "<1-2 phrases en français expliquant le choix, adressé à l'utilisateur>"}

            Règles :
            - match_id = null si AUCUN psychologue ne couvre raisonnablement la problématique.
            - Ne force jamais un match si les bios ne contiennent rien de pertinent.
            - reasoning : tutoie/vouvoie selon le ton de l'échange ; reste chaleureux, concret, bref.
            """;

        var msgs = history.Select(m => (m.Role, m.Content));
        var raw = await _claude.CompleteAsync(system, msgs, 600, ct);

        int? matchId = null;
        string reasoning = "";
        try
        {
            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                var json = raw.Substring(start, end - start + 1);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("match_id", out var m) && m.ValueKind == JsonValueKind.Number)
                    matchId = m.GetInt32();
                if (doc.RootElement.TryGetProperty("reasoning", out var r))
                    reasoning = r.GetString() ?? "";
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to parse Claude match JSON: {Raw}", raw);
        }

        if (matchId is int id && psys.Any(p => p.Id == id))
        {
            var p = psys.First(x => x.Id == id);
            var reply = string.IsNullOrWhiteSpace(reasoning)
                ? $"Je vous recommande {p.Name} ({p.Title})."
                : reasoning;
            return new ChatResponse(reply, "done", id, reasoning);
        }

        var msg = string.IsNullOrWhiteSpace(reasoning)
            ? "Je n'ai personne qui corresponde parfaitement aujourd'hui. Voulez-vous ajouter des précisions ?"
            : reasoning + " Voulez-vous ajouter des précisions à votre demande ?";
        return new ChatResponse(msg, "awaiting_followup", null, reasoning);
    }
}
