using System.ComponentModel.DataAnnotations;

namespace Psy.Api.Models;

public class SkillIcon
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string Icon { get; set; } = "psychology";
}
