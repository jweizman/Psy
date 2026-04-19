using Microsoft.EntityFrameworkCore;
using Psy.Api.Data;
using Psy.Api.Models;
using Psy.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=psy.db"));

builder.Services.AddHttpClient<ClaudeClient>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddScoped<SentimentService>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

static PsychologistDto ToDto(Psychologist p) => new(
    p.Id, p.Name, p.Title, p.PhotoUrl, p.Bio,
    string.IsNullOrWhiteSpace(p.SkillsCsv)
        ? Array.Empty<string>()
        : p.SkillsCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
    p.Rating, p.ReviewCount, p.NextAvailable, p.Verified, p.AvailableToday);

var api = app.MapGroup("/api");

api.MapGet("/psychologists", async (AppDbContext db, string? q) =>
{
    var query = db.Psychologists.AsQueryable();
    if (!string.IsNullOrWhiteSpace(q))
    {
        var lower = q.ToLower();
        query = query.Where(p =>
            p.Name.ToLower().Contains(lower) ||
            p.Title.ToLower().Contains(lower) ||
            p.SkillsCsv.ToLower().Contains(lower) ||
            p.Bio.ToLower().Contains(lower));
    }
    var items = await query.OrderByDescending(p => p.Rating).ToListAsync();
    return Results.Ok(items.Select(ToDto));
});

api.MapGet("/psychologists/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Psychologists.FindAsync(id);
    return p is null ? Results.NotFound() : Results.Ok(ToDto(p));
});

api.MapPost("/psychologists", async (PsychologistInput input, AppDbContext db) =>
{
    var p = new Psychologist
    {
        Name = input.Name,
        Title = input.Title,
        PhotoUrl = input.PhotoUrl,
        Bio = input.Bio,
        SkillsCsv = string.Join(",", input.Skills ?? Array.Empty<string>()),
        Rating = input.Rating,
        ReviewCount = input.ReviewCount,
        NextAvailable = input.NextAvailable,
        Verified = input.Verified,
        AvailableToday = input.AvailableToday
    };
    db.Psychologists.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/api/psychologists/{p.Id}", ToDto(p));
});

api.MapPut("/psychologists/{id:int}", async (int id, PsychologistInput input, AppDbContext db) =>
{
    var p = await db.Psychologists.FindAsync(id);
    if (p is null) return Results.NotFound();
    p.Name = input.Name;
    p.Title = input.Title;
    p.PhotoUrl = input.PhotoUrl;
    p.Bio = input.Bio;
    p.SkillsCsv = string.Join(",", input.Skills ?? Array.Empty<string>());
    p.Rating = input.Rating;
    p.ReviewCount = input.ReviewCount;
    p.NextAvailable = input.NextAvailable;
    p.Verified = input.Verified;
    p.AvailableToday = input.AvailableToday;
    await db.SaveChangesAsync();
    return Results.Ok(ToDto(p));
});

api.MapDelete("/psychologists/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Psychologists.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Reviews.RemoveRange(db.Reviews.Where(r => r.PsychologistId == id));
    db.Slots.RemoveRange(db.Slots.Where(s => s.PsychologistId == id));
    db.Psychologists.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

static ReviewDto ToReviewDto(Review r) => new(
    r.Id, r.PsychologistId, r.Author, r.Initials, r.Rating, r.Text, r.CreatedAt);

api.MapGet("/psychologists/{id:int}/reviews", async (int id, AppDbContext db) =>
{
    var items = await db.Reviews
        .Where(r => r.PsychologistId == id)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();
    return Results.Ok(items.Select(ToReviewDto));
});

api.MapPost("/psychologists/{id:int}/reviews", async (int id, ReviewInput input, AppDbContext db) =>
{
    if (await db.Psychologists.FindAsync(id) is null) return Results.NotFound();
    var r = new Review
    {
        PsychologistId = id,
        Author = input.Author,
        Initials = string.IsNullOrWhiteSpace(input.Initials)
            ? new string((input.Author ?? "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(w => w[0])
                .ToArray())
                .ToUpperInvariant()
            : input.Initials,
        Rating = Math.Clamp(input.Rating, 1, 5),
        Text = input.Text,
        CreatedAt = DateTime.UtcNow
    };
    db.Reviews.Add(r);
    await db.SaveChangesAsync();
    return Results.Created($"/api/reviews/{r.Id}", ToReviewDto(r));
});

api.MapDelete("/reviews/{id:int}", async (int id, AppDbContext db) =>
{
    var r = await db.Reviews.FindAsync(id);
    if (r is null) return Results.NotFound();
    db.Reviews.Remove(r);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

static SlotDto ToSlotDto(AvailabilitySlot s) => new(
    s.Id, s.PsychologistId, s.StartUtc, s.DurationMinutes, s.Booked);

api.MapGet("/psychologists/{id:int}/slots", async (int id, AppDbContext db) =>
{
    var now = DateTime.UtcNow;
    var items = await db.Slots
        .Where(s => s.PsychologistId == id && s.StartUtc >= now)
        .OrderBy(s => s.StartUtc)
        .ToListAsync();
    return Results.Ok(items.Select(ToSlotDto));
});

api.MapPost("/psychologists/{id:int}/slots", async (int id, SlotInput input, AppDbContext db) =>
{
    if (await db.Psychologists.FindAsync(id) is null) return Results.NotFound();
    var s = new AvailabilitySlot
    {
        PsychologistId = id,
        StartUtc = input.StartUtc.ToUniversalTime(),
        DurationMinutes = input.DurationMinutes <= 0 ? 45 : input.DurationMinutes,
        Booked = input.Booked
    };
    db.Slots.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/api/slots/{s.Id}", ToSlotDto(s));
});

api.MapDelete("/slots/{id:int}", async (int id, AppDbContext db) =>
{
    var s = await db.Slots.FindAsync(id);
    if (s is null) return Results.NotFound();
    db.Slots.Remove(s);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

static SkillIconDto ToSkillIconDto(SkillIcon s) => new(s.Id, s.Name, s.Icon);

api.MapGet("/skill-icons", async (AppDbContext db) =>
{
    var items = await db.SkillIcons.OrderBy(s => s.Name).ToListAsync();
    return Results.Ok(items.Select(ToSkillIconDto));
});

api.MapPost("/skill-icons", async (SkillIconInput input, AppDbContext db) =>
{
    var name = (input.Name ?? "").Trim();
    if (name.Length == 0) return Results.BadRequest("Name required.");
    var existing = await db.SkillIcons.FirstOrDefaultAsync(s => s.Name == name);
    if (existing is not null)
    {
        existing.Icon = input.Icon;
        await db.SaveChangesAsync();
        return Results.Ok(ToSkillIconDto(existing));
    }
    var s = new SkillIcon { Name = name, Icon = input.Icon };
    db.SkillIcons.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/api/skill-icons/{s.Id}", ToSkillIconDto(s));
});

api.MapPut("/skill-icons/{id:int}", async (int id, SkillIconInput input, AppDbContext db) =>
{
    var s = await db.SkillIcons.FindAsync(id);
    if (s is null) return Results.NotFound();
    s.Name = (input.Name ?? s.Name).Trim();
    s.Icon = input.Icon;
    await db.SaveChangesAsync();
    return Results.Ok(ToSkillIconDto(s));
});

api.MapDelete("/skill-icons/{id:int}", async (int id, AppDbContext db) =>
{
    var s = await db.SkillIcons.FindAsync(id);
    if (s is null) return Results.NotFound();
    db.SkillIcons.Remove(s);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapPost("/chat", async (ChatRequest req, MatchingService svc, CancellationToken ct) =>
{
    try
    {
        var res = await svc.ProcessAsync(req, ct);
        return Results.Ok(res);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

api.MapPost("/sentiment", async (SentimentRequest req, SentimentService svc, CancellationToken ct) =>
{
    try
    {
        var res = await svc.AnalyzeAsync(req.Text ?? "", ct);
        return Results.Ok(res);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

api.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
