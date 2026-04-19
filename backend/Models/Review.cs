using System.ComponentModel.DataAnnotations;

namespace Psy.Api.Models;

public class Review
{
    public int Id { get; set; }
    public int PsychologistId { get; set; }

    [Required, MaxLength(80)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(4)]
    public string Initials { get; set; } = string.Empty;

    public int Rating { get; set; } = 5;

    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
