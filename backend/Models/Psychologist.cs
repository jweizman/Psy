using System.ComponentModel.DataAnnotations;

namespace Psy.Api.Models;

public class Psychologist
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PhotoUrl { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string SkillsCsv { get; set; } = string.Empty;

    public double Rating { get; set; }
    public int ReviewCount { get; set; }

    [MaxLength(50)]
    public string NextAvailable { get; set; } = string.Empty;

    public bool Verified { get; set; }
    public bool AvailableToday { get; set; } = true;
}
