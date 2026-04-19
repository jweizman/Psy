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
                Bio = "Psychologue clinicienne spécialisée dans la gestion du stress chronique, des troubles anxieux généralisés et des attaques de panique. Approche intégrative mêlant TCC, pleine conscience et techniques de régulation émotionnelle. 12 ans d'expérience en cabinet et en milieu hospitalier.",
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
                Bio = "Neuropsychologue, évaluation et prise en charge du TDAH chez l'adulte et l'adolescent. Travail sur la procrastination, la dysrégulation attentionnelle, les fonctions exécutives et l'estime de soi liée au TDAH. Thérapie cognitivo-comportementale et remédiation cognitive.",
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
                Bio = "Spécialiste du psychotraumatisme, certifiée EMDR Europe. Prise en charge des traumatismes complexes, deuils, violences, et syndromes post-traumatiques. Approche intégrative centrée sur la personne et la sécurité thérapeutique.",
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
