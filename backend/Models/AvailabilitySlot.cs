namespace Psy.Api.Models;

public class AvailabilitySlot
{
    public int Id { get; set; }
    public int PsychologistId { get; set; }
    public DateTime StartUtc { get; set; }
    public int DurationMinutes { get; set; } = 45;
    public bool Booked { get; set; }
}
