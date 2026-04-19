using Psy.Api.Models;

namespace Psy.Api.Data;

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Psychologists.Any()) return;

        db.Psychologists.AddRange(
            new Psychologist
            {
                Name = "Dr. Camille Laurent",
                Title = "Stress & Anxiety Specialist",
                PhotoUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuCJGtd5b1POvJtQJu2H2plPTmTIcy5EQXmf8HUuWOpIASp0_wMqN1sMatr12z4sm16bt1gR1yHIbF3b7gre7LKeORpZN7cfIWLGYzT-tRFrHz34_BdsBHUOqFLrBI7wSVAV24XmZYMhp03LtxtkGuwRQGk7OFBsda_qUWbiZJSVpozi-wHH26-P8N7bIogTNL9xbgdKnK880PjAlTV5oaxNCHxuEdvfZl5jYEJ_HwJkBYrW0ImK7hVdmcIJeA5byrUxjInGMO3h0hSS",
                Bio = "Clinical psychologist specialised in chronic stress management, generalised anxiety disorders and panic attacks. Integrative approach combining CBT, mindfulness and emotional-regulation techniques. 12 years of experience in private practice and hospital settings.",
                SkillsCsv = "stress,anxiety,panic,burnout,mindfulness,CBT,emotional-regulation",
                Rating = 4.9,
                ReviewCount = 124,
                NextAvailable = "14:00",
                Verified = true,
                AvailableToday = true
            },
            new Psychologist
            {
                Name = "Marc-Antoine Roux",
                Title = "ADHD & Behavioral Cognition",
                PhotoUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuApIaiCA5ieu8ZAUMlHyTq51Gqw61FG19H8fGeKlr-ksbZ6ZvgG9IrqcLmDjJveC7utWrfZnYXV51_PcRgVa8HELWIXOM4TsoMfujl3GQElUUkzguOZra74e4_CVp802y1jL-UswFpBTcT89a0gITfjv7R2V4j-vds2tjcqmddX_Xv97BthihBJXvvvpvQg2fduqqB-g0eYp44JuoG3Lnkzg8MV6xAOaqTlG3_YkAd79SE_iFdXJY8GEWSXTcn5yTOnjhv8alyqZevI",
                Bio = "Neuropsychologist. Assessment and treatment of ADHD in adults and adolescents. Works on procrastination, attention dysregulation, executive function and ADHD-related self-esteem. Cognitive-behavioural therapy and cognitive remediation.",
                SkillsCsv = "ADHD,attention,executive-function,procrastination,self-esteem,CBT,adolescent",
                Rating = 4.8,
                ReviewCount = 89,
                NextAvailable = "16:30",
                Verified = true,
                AvailableToday = true
            },
            new Psychologist
            {
                Name = "Dr. Inès Moreau",
                Title = "Trauma & EMDR Therapist",
                PhotoUrl = "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?w=400",
                Bio = "Trauma specialist, EMDR Europe certified. Treatment of complex trauma, grief, violence, and post-traumatic stress disorder. Integrative person-centred approach grounded in therapeutic safety.",
                SkillsCsv = "trauma,PTSD,EMDR,grief,violence,attachment",
                Rating = 4.95,
                ReviewCount = 210,
                NextAvailable = "10:00",
                Verified = true,
                AvailableToday = true
            }
        );
        db.SaveChanges();
    }
}
