using Microsoft.EntityFrameworkCore;
using Psy.Api.Models;

namespace Psy.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Psychologist> Psychologists => Set<Psychologist>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AvailabilitySlot> Slots => Set<AvailabilitySlot>();
    public DbSet<SkillIcon> SkillIcons => Set<SkillIcon>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<SkillIcon>().HasIndex(s => s.Name).IsUnique();
        mb.Entity<Review>().HasIndex(r => r.PsychologistId);
        mb.Entity<AvailabilitySlot>().HasIndex(s => new { s.PsychologistId, s.StartUtc });
    }
}
