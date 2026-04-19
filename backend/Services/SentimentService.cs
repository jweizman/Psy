using System.Text.Json;

namespace Psy.Api.Services;

public record SentimentResult(
    string Label,
    int Intensity,
    string[] Emotions,
    string Note);

public class SentimentService
{
    private readonly ClaudeClient _claude;
    private readonly ILogger<SentimentService> _log;

    public SentimentService(ClaudeClient claude, ILogger<SentimentService> log)
    {
        _claude = claude;
        _log = log;
    }

    public async Task<SentimentResult> AnalyzeAsync(string text, CancellationToken ct)
    {
        if (!_claude.IsConfigured || string.IsNullOrWhiteSpace(text))
            return new SentimentResult("neutral", 0, Array.Empty<string>(), "");

        const string system = """
            You analyze a short message sent by a user to a psychology-matching app.
            Return STRICT JSON (no prose, no markdown) in this shape:
            {
              "label": "positive" | "neutral" | "negative",
              "intensity": <integer 1-5, 5 = very strong>,
              "emotions": ["<lowercase emotion>", ...],  // 0-4 items, e.g. anxiety, sadness, anger, shame, hope, relief, overwhelm, numbness
              "note": "<one short sentence in English>"
            }
            Focus on affect cues in the text. Be conservative: if unclear, use neutral + low intensity.
            Never diagnose. Never mention the word "diagnosis".
            """;

        var raw = await _claude.CompleteAsync(
            system,
            new[] { ("user", text) },
            300,
            ct);

        try
        {
            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            if (start < 0 || end <= start) return Fallback();
            var json = raw.Substring(start, end - start + 1);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var label = root.TryGetProperty("label", out var l) ? (l.GetString() ?? "neutral") : "neutral";
            if (label != "positive" && label != "neutral" && label != "negative") label = "neutral";

            var intensity = 0;
            if (root.TryGetProperty("intensity", out var i) && i.ValueKind == JsonValueKind.Number)
                intensity = Math.Clamp(i.GetInt32(), 0, 5);

            var emotions = Array.Empty<string>();
            if (root.TryGetProperty("emotions", out var e) && e.ValueKind == JsonValueKind.Array)
                emotions = e.EnumerateArray()
                    .Select(x => x.GetString() ?? "")
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Take(4)
                    .ToArray();

            var note = root.TryGetProperty("note", out var n) ? (n.GetString() ?? "") : "";

            return new SentimentResult(label, intensity, emotions, note);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to parse sentiment JSON: {Raw}", raw);
            return Fallback();
        }

        static SentimentResult Fallback() =>
            new("neutral", 0, Array.Empty<string>(), "");
    }
}
