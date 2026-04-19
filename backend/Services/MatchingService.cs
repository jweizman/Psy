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
                "The chat service is not configured (Anthropic API key missing). Please contact the administrator.",
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
            You are the intake assistant for a psychological-support platform.
            At this step your ONLY job is to MIRROR the user's concern: rephrase it
            as a single clear sentence, stripped of jargon and extraneous context,
            then ask for confirmation.
            Style: warm, concise, in English.
            REQUIRED format:
            "If I understand correctly, you are going through: <short rephrasing>. Did I get that right?"
            Do not add anything else. Do not give advice. Do not suggest a psychologist yet.
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
                "Alright, take care of yourself. Come back whenever you like.",
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
                "No one is available at the moment. Would you like to add more detail so we can look more broadly?",
                "awaiting_followup", null, null);
        }

        var catalog = new StringBuilder();
        foreach (var p in psys)
        {
            catalog.AppendLine($"- id={p.Id} | {p.Name} | {p.Title}");
            catalog.AppendLine($"  next_available: {p.NextAvailable}");
            catalog.AppendLine($"  skills: {p.SkillsCsv}");
            catalog.AppendLine($"  bio: {p.Bio}");
        }

        var system = $$"""
            You are a matching assistant for a psychology platform.
            Here is the list of psychologists AVAILABLE TODAY:

            {{catalog}}

            Your job: given the user's concern, pick AT MOST ONE practitioner whose
            bio/skills explicitly address what the user described. If nothing in any
            bio genuinely covers the concern, return null — do not force a match.

            Respond STRICTLY as valid JSON, with no surrounding text, in this shape:
            {"match_id": <int or null>, "reasoning": "<2-3 sentences addressed to the user>"}

            Rules for `reasoning`:
            - Start by naming the practitioner and their specialty.
            - Then ground the recommendation in SPECIFIC wording pulled from that
              practitioner's bio or skills (e.g. "Their bio mentions X, which fits
              what you described about Y"). The link between the user's concern and
              the bio must be explicit.
            - End with a concrete next step (e.g. "Would you like me to book their
              next slot at <NextAvailable>?").
            - Warm, second person, brief. No invented credentials.
            - If match_id is null, explain briefly why no one in the list fits and
              invite the user to add detail.
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
                ? $"I'd recommend {p.Name} ({p.Title})."
                : reasoning;
            return new ChatResponse(reply, "done", id, reasoning);
        }

        var msg = string.IsNullOrWhiteSpace(reasoning)
            ? "I don't have anyone who fits perfectly today. Would you like to add more detail?"
            : reasoning + " Would you like to add more detail to your request?";
        return new ChatResponse(msg, "awaiting_followup", null, reasoning);
    }
}
