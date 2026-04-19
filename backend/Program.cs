using Microsoft.EntityFrameworkCore;
using Psy.Api.Data;
using Psy.Api.Models;
using Psy.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=psy.db"));

builder.Services.AddHttpClient<ClaudeClient>();
builder.Services.AddScoped<MatchingService>();

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
    db.Psychologists.Remove(p);
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

api.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
