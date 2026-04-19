using Microsoft.EntityFrameworkCore;
using Psy.Api.Models;

namespace Psy.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Psychologist> Psychologists => Set<Psychologist>();
}
