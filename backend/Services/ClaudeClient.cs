using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Psy.Api.Services;

public class ClaudeClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public ClaudeClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Anthropic:ApiKey"]
                  ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
                  ?? "";
        _model = config["Anthropic:Model"] ?? "claude-sonnet-4-6";
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string> CompleteAsync(
        string system,
        IEnumerable<(string role, string content)> messages,
        int maxTokens = 1024,
        CancellationToken ct = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("ANTHROPIC_API_KEY not configured.");

        var payload = new
        {
            model = _model,
            max_tokens = maxTokens,
            system,
            messages = messages.Select(m => new { role = m.role, content = m.content }).ToArray()
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        req.Headers.Add("x-api-key", _apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        req.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Claude API error {(int)res.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement.GetProperty("content");
        var sb = new StringBuilder();
        foreach (var block in content.EnumerateArray())
        {
            if (block.TryGetProperty("type", out var t) && t.GetString() == "text")
                sb.Append(block.GetProperty("text").GetString());
        }
        return sb.ToString().Trim();
    }
}
