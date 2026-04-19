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

public record ReviewDto(
    int Id,
    int PsychologistId,
    string Author,
    string Initials,
    int Rating,
    string Text,
    DateTime CreatedAt);

public record ReviewInput(
    string Author,
    string Initials,
    int Rating,
    string Text);

public record SlotDto(
    int Id,
    int PsychologistId,
    DateTime StartUtc,
    int DurationMinutes,
    bool Booked);

public record SlotInput(
    DateTime StartUtc,
    int DurationMinutes,
    bool Booked);

public record SkillIconDto(int Id, string Name, string Icon);

public record SkillIconInput(string Name, string Icon);
