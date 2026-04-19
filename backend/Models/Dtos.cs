namespace Psy.Api.Models;

public record PsychologistDto(
    int Id,
    string Name,
    string Title,
    string PhotoUrl,
    string Bio,
    string[] Skills,
    double Rating,
    int ReviewCount,
    string NextAvailable,
    bool Verified,
    bool AvailableToday);

public record PsychologistInput(
    string Name,
    string Title,
    string PhotoUrl,
    string Bio,
    string[] Skills,
    double Rating,
    int ReviewCount,
    string NextAvailable,
    bool Verified,
    bool AvailableToday);

public record ChatMessage(string Role, string Content);

public record ChatRequest(List<ChatMessage> History, string Phase);

public record ChatResponse(
    string Reply,
    string Phase,
    int? MatchedPsychologistId,
    string? Reasoning);
