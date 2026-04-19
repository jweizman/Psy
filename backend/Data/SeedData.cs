using Microsoft.EntityFrameworkCore;
using Psy.Api.Models;

namespace Psy.Api.Data;

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        db.Database.EnsureCreated();
        EnsureLegacyTables(db);

        if (!db.SkillIcons.Any())
        {
            db.SkillIcons.AddRange(
                new SkillIcon { Name = "stress", Icon = "psychology" },
                new SkillIcon { Name = "anxiety", Icon = "favorite" },
                new SkillIcon { Name = "panic", Icon = "bolt" },
                new SkillIcon { Name = "burnout", Icon = "work_history" },
                new SkillIcon { Name = "mindfulness", Icon = "self_improvement" },
                new SkillIcon { Name = "CBT", Icon = "neurology" },
                new SkillIcon { Name = "emotional-regulation", Icon = "mood" },
                new SkillIcon { Name = "ADHD", Icon = "target" },
                new SkillIcon { Name = "attention", Icon = "visibility" },
                new SkillIcon { Name = "executive-function", Icon = "checklist" },
                new SkillIcon { Name = "procrastination", Icon = "schedule" },
                new SkillIcon { Name = "self-esteem", Icon = "auto_awesome" },
                new SkillIcon { Name = "adolescent", Icon = "school" },
                new SkillIcon { Name = "trauma", Icon = "healing" },
                new SkillIcon { Name = "PTSD", Icon = "healing" },
                new SkillIcon { Name = "EMDR", Icon = "remove_red_eye" },
                new SkillIcon { Name = "grief", Icon = "sentiment_very_dissatisfied" },
                new SkillIcon { Name = "violence", Icon = "shield" },
                new SkillIcon { Name = "attachment", Icon = "link" },
                new SkillIcon { Name = "sleep", Icon = "bedtime" }
            );
            db.SaveChanges();
        }

        if (db.Psychologists.Any()) return;

        var camille = new Psychologist
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
        };
        var marc = new Psychologist
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
        };
        var ines = new Psychologist
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
        };

        db.Psychologists.AddRange(camille, marc, ines);
        db.SaveChanges();

        db.Reviews.AddRange(
            new Review { PsychologistId = camille.Id, Author = "Marc D.", Initials = "MD", Rating = 5, Text = "Exceptional listening skills. I was finally able to put words to my professional burnout." },
            new Review { PsychologistId = camille.Id, Author = "Sophie L.", Initials = "SL", Rating = 5, Text = "Very gentle and professional. The teleconsultation format is really convenient." },
            new Review { PsychologistId = marc.Id, Author = "Julien R.", Initials = "JR", Rating = 5, Text = "Finally someone who understands adult ADHD. Practical and kind." },
            new Review { PsychologistId = ines.Id, Author = "Anna P.", Initials = "AP", Rating = 5, Text = "The EMDR sessions were life-changing. I felt safe throughout." }
        );

        SeedSlotsFor(db, camille.Id);
        SeedSlotsFor(db, marc.Id);
        SeedSlotsFor(db, ines.Id);

        db.SaveChanges();

        EnsureSpecialists(db);
    }

    private static void SeedSlotsFor(AppDbContext db, int psyId)
    {
        var today = DateTime.UtcNow.Date;
        var hours = new[] { 9, 10, 14, 15, 17, 18 };
        for (int d = 0; d < 5; d++)
        {
            foreach (var h in hours)
            {
                db.Slots.Add(new AvailabilitySlot
                {
                    PsychologistId = psyId,
                    StartUtc = today.AddDays(d).AddHours(h),
                    DurationMinutes = 45,
                    Booked = (d + h) % 5 == 0
                });
            }
        }
    }

    private static void EnsureSkillIcon(AppDbContext db, string name, string icon)
    {
        if (db.SkillIcons.Any(s => s.Name == name)) return;
        db.SkillIcons.Add(new SkillIcon { Name = name, Icon = icon });
    }

    private static Psychologist? EnsurePractitioner(
        AppDbContext db,
        string name,
        string title,
        string photoUrl,
        string bio,
        string skillsCsv,
        double rating,
        int reviewCount,
        string nextAvailable)
    {
        if (db.Psychologists.Any(p => p.Name == name)) return null;
        var p = new Psychologist
        {
            Name = name,
            Title = title,
            PhotoUrl = photoUrl,
            Bio = bio,
            SkillsCsv = skillsCsv,
            Rating = rating,
            ReviewCount = reviewCount,
            NextAvailable = nextAvailable,
            Verified = true,
            AvailableToday = true
        };
        db.Psychologists.Add(p);
        return p;
    }

    private static void EnsureSpecialists(AppDbContext db)
    {
        var extraIcons = new (string Name, string Icon)[]
        {
            ("couples", "diversity_1"),
            ("gottman-method", "handshake"),
            ("communication", "forum"),
            ("conflict-resolution", "handshake"),
            ("trust-repair", "volunteer_activism"),
            ("pre-marital", "favorite"),
            ("infidelity", "heart_broken"),
            ("imago", "forum"),
            ("childhood-wounds", "child_care"),
            ("dialogue", "forum"),
            ("EFT", "mood_heart"),
            ("LGBTQ", "diversity_2"),
            ("trauma-informed", "healing"),
            ("sex-therapy", "favorite"),
            ("desire-discrepancy", "sync_problem"),
            ("body-image", "self_improvement"),
            ("systemic-therapy", "hub"),
            ("blended-family", "family_restroom"),
            ("separation", "heart_broken"),
            ("co-parenting", "family_restroom"),
            ("step-parenting", "family_restroom"),
            ("combat-trauma", "shield"),
            ("prolonged-exposure", "visibility"),
            ("veterans", "military_tech"),
            ("moral-injury", "balance"),
            ("hypervigilance", "visibility"),
            ("complex-PTSD", "healing"),
            ("refugees", "luggage"),
            ("displacement", "flight_land"),
            ("narrative-exposure", "menu_book"),
            ("torture", "shield"),
            ("CPT", "neurology"),
            ("reintegration", "restart_alt"),
            ("children-adolescents", "school"),
            ("child-soldiers", "shield"),
            ("group-therapy", "groups"),
            ("resilience", "emoji_events"),
        };
        foreach (var (n, i) in extraIcons) EnsureSkillIcon(db, n, i);
        db.SaveChanges();

        var specialists = new List<(
            string Name, string Title, string Photo, string Bio, string Skills,
            double Rating, int Reviews, string NextAvailable)>
        {
            (
                "Dr. Léa Benoist",
                "Couples Therapist · Gottman Method",
                "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=400&h=500&fit=crop",
                "Gottman Method Couples Therapist, Level 3 certified, trained at the Gottman Institute in Seattle with Dr. John and Dr. Julie Gottman. 14 years working with distressed couples, including partners recovering from infidelity, chronic criticism and emotional flooding. Sessions combine structured Sound Relationship House interventions (Love Maps, rituals of connection, conflict mapping) with physiological self-regulation work — particularly the Gottman Repair Checklist and '20-minute break' protocols when sessions escalate. Also specialises in pre-marital counselling for engaged partners and for couples navigating the transition to parenthood. Bilingual English/French; I see partners both individually during the first intake and jointly afterwards.",
                "couples,gottman-method,communication,conflict-resolution,attachment,trust-repair,pre-marital,infidelity",
                4.9, 186, "11:00"
            ),
            (
                "Théo Vasseur",
                "Imago Relationship Therapist",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=500&fit=crop",
                "Certified Imago Relationship Therapist (CIRT) since 2016, trained with Harville Hendrix's European faculty. I work with couples who keep replaying the same fight — the one where the words change but the wound underneath does not. Core tool: the structured Imago Dialogue (mirror, validate, empathise) used to slow reactive communication and surface each partner's unmet developmental needs. Particular focus on couples in which one or both partners grew up with emotionally unavailable caregivers, and on long-term partners (15+ years) facing disconnection rather than overt conflict. I also co-facilitate monthly 'Getting the Love You Want' weekend workshops.",
                "couples,imago,communication,attachment,intimacy,childhood-wounds,dialogue",
                4.85, 97, "15:30"
            ),
            (
                "Dr. Naïma Oubba",
                "EFT Couples Therapist · Attachment Specialist",
                "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400&h=500&fit=crop",
                "PhD in clinical psychology, certified Emotionally Focused Couples Therapist (ICEEFT) and supervisor-in-training under Dr. Sue Johnson's framework. I guide couples through the three EFT stages — de-escalation of negative cycles, restructuring of attachment injuries (including the Attachment Injury Resolution Model), and consolidation of new bonding interactions. Specialised experience with same-sex and non-binary couples, couples after infidelity, and partners where one is managing a chronic illness. Trauma-informed: I screen for attachment trauma and adjust pacing accordingly. 11 years of practice; trilingual (French, Arabic, English).",
                "couples,EFT,attachment,emotional-regulation,intimacy,trauma-informed,LGBTQ",
                4.92, 142, "09:30"
            ),
            (
                "Sarah Chen-Morel",
                "Certified Sex & Couples Therapist",
                "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=400&h=500&fit=crop&crop=faces",
                "AASECT-certified sex therapist and licensed couples therapist. 9 years working on sexual dysfunction within the relational context — desire discrepancy, erectile concerns, vaginismus, pain during intercourse, and post-partum intimacy shifts. Approach integrates the Dual Control Model (Bancroft & Janssen), sensate focus exercises (Masters & Johnson), and cognitive-behavioural sex therapy. I routinely work with mixed-orientation couples, open-relationship couples (including polyamorous and CNM frameworks), and partners rebuilding physical intimacy after cancer treatment or gender-affirming surgery. Sessions are explicit, body-literate and non-judgemental.",
                "couples,sex-therapy,intimacy,desire-discrepancy,LGBTQ,communication,body-image",
                4.88, 73, "16:00"
            ),
            (
                "Dr. Pierre Aubry",
                "Systemic Family & Couples Therapist",
                "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=400&h=500&fit=crop",
                "Systemic and family therapist, 22 years of practice, trained at the Milan school and at the Mental Research Institute (Palo Alto) brief-therapy program. I work with couples through the lens of the broader family system: extended-family pressure, ex-partners, stepchildren loyalty conflicts, and triangulation patterns. Particular expertise with blended families (remarriage, stepparent–stepchild alliances), couples considering separation who want a structured 'last-chance' intensive, and co-parenting after divorce. I use genograms in session one and circular questioning throughout. I also conduct mediated separation conversations when reconciliation is not the goal.",
                "couples,systemic-therapy,blended-family,separation,co-parenting,conflict-resolution,step-parenting",
                4.78, 211, "14:30"
            ),
            (
                "Dr. Yossi Shahar",
                "Combat Trauma & PTSD Specialist",
                "https://images.unsplash.com/photo-1605614796473-f37a7b9dc14e?w=400&h=500&fit=crop",
                "Clinical psychologist and former IDF medical-corps reservist psychologist, 15 years treating soldiers, reservists and civilians exposed to combat, rocket attacks and terror events. Trained in Prolonged Exposure (PE) therapy under Edna Foa's protocol, EMDR (EMDRIA-certified), and Somatic Experiencing (SEP). Treatment focus: re-experiencing symptoms (nightmares, flashbacks), hyperarousal, avoidance, and the less-discussed 'homecoming numbness'. Also works on moral injury — the psychological cost of acts that violate one's own ethical code during warfare — using Adaptive Disclosure (Litz). Languages: Hebrew, English, French.",
                "PTSD,combat-trauma,EMDR,prolonged-exposure,veterans,moral-injury,hypervigilance",
                4.95, 168, "10:00"
            ),
            (
                "Dr. Iryna Kovalenko",
                "War Refugee & Displacement Trauma Psychologist",
                "https://images.unsplash.com/photo-1551836022-deb4988cc6c0?w=400&h=500&fit=crop",
                "Ukrainian-trained clinical psychologist, relocated in 2022. I work with refugees, internally displaced persons and diaspora families from Ukraine, Syria, Afghanistan and Sudan. 12 years of practice including 4 years embedded with humanitarian NGOs on the ground. Primary modality: Narrative Exposure Therapy (NET — Schauer/Neuner/Elbert), designed for survivors of cumulative traumatic events and organised violence. Also EMDR-trained, with adaptations for survivors who cannot safely remain in the memory for standard bilateral processing. Expertise: survivor guilt, ambiguous loss (missing relatives, disappeared bodies), intergenerational transmission, and cultural bereavement. Works in Ukrainian, Russian, English, basic Polish.",
                "PTSD,complex-PTSD,refugees,displacement,grief,EMDR,narrative-exposure",
                4.93, 124, "13:00"
            ),
            (
                "Mahmoud Al-Haddad",
                "Torture & Displacement Trauma Therapist",
                "https://images.unsplash.com/photo-1581579438747-104c53e7a3ab?w=400&h=500&fit=crop",
                "MA in psychology, trained at the IRCT (International Rehabilitation Council for Torture Victims) and the Centre Primo Levi in Paris. 10 years working exclusively with survivors of torture, political imprisonment and forced disappearance — mostly from Syria, Iraq, Yemen, Eritrea and Iran. Works within the IRCT four-pillar approach: medical, psychological, social, legal referral. Primary therapeutic tools: trauma-focused CBT adapted for survivors, Testimony Therapy (Cienfuegos & Monelli), and stabilisation-first Phase-Based treatment (Herman / Van der Hart). I collaborate regularly with asylum lawyers and immigration physicians on forensic documentation (Istanbul Protocol). Languages: Arabic, English, French.",
                "PTSD,complex-PTSD,torture,refugees,trauma-informed,grief",
                4.89, 88, "17:00"
            ),
            (
                "Dr. Rebecca Morrison",
                "Veterans & Moral Injury Specialist",
                "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400&h=500&fit=crop",
                "PhD clinical psychology, 16 years at US Department of Veterans Affairs (VA) medical centres treating post-9/11 combat veterans from Iraq, Afghanistan and Syria deployments, as well as Vietnam-era veterans presenting decades later. Certified in all three VA-endorsed trauma modalities: Prolonged Exposure (PE), Cognitive Processing Therapy (CPT), and EMDR. Particular depth in moral injury — guilt, shame and loss of meaning tied to acts committed, witnessed or failed to prevent — using the Building Spiritual Strength protocol and Adaptive Disclosure. Also works on military-to-civilian reintegration, service-dog integration for PTSD, and partners of veterans (secondary traumatic stress). English only.",
                "PTSD,veterans,moral-injury,combat-trauma,CPT,prolonged-exposure,reintegration",
                4.9, 257, "08:30"
            ),
            (
                "Dr. Amina Diop",
                "Humanitarian & Child Soldier Reintegration Psychologist",
                "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=400&h=500&fit=crop",
                "Humanitarian clinical psychologist, 13 years with the ICRC and UNICEF programmes across DRC, CAR, South Sudan and northern Mali. Focus: children and adolescents associated with armed forces or armed groups (CAAFAG), unaccompanied minor refugees, and survivors of conflict-related sexual violence. Modalities: Trauma-Focused CBT for children, KIDNET (a child adaptation of Narrative Exposure Therapy), community-based sociodrama, and structured group interventions to rebuild peer trust. I also train in-country counsellors and supervise field teams. Works in French, English, Wolof, Bambara and functional Lingala.",
                "PTSD,children-adolescents,refugees,child-soldiers,trauma-informed,group-therapy,resilience",
                4.94, 131, "15:00"
            ),
        };

        var added = new List<Psychologist>();
        foreach (var s in specialists)
        {
            var p = EnsurePractitioner(db, s.Name, s.Title, s.Photo, s.Bio, s.Skills, s.Rating, s.Reviews, s.NextAvailable);
            if (p is not null) added.Add(p);
        }
        if (added.Count == 0) return;
        db.SaveChanges();

        foreach (var p in added) SeedSlotsFor(db, p.Id);
        db.SaveChanges();
    }

    private static void EnsureLegacyTables(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "SkillIcons" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_SkillIcons" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Icon" TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_SkillIcons_Name" ON "SkillIcons" ("Name");

            CREATE TABLE IF NOT EXISTS "Reviews" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Reviews" PRIMARY KEY AUTOINCREMENT,
                "PsychologistId" INTEGER NOT NULL,
                "Author" TEXT NOT NULL,
                "Initials" TEXT NULL,
                "Rating" INTEGER NOT NULL,
                "Text" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS "IX_Reviews_PsychologistId" ON "Reviews" ("PsychologistId");

            CREATE TABLE IF NOT EXISTS "Slots" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Slots" PRIMARY KEY AUTOINCREMENT,
                "PsychologistId" INTEGER NOT NULL,
                "StartUtc" TEXT NOT NULL,
                "DurationMinutes" INTEGER NOT NULL,
                "Booked" INTEGER NOT NULL
            );
            CREATE INDEX IF NOT EXISTS "IX_Slots_PsychologistId_StartUtc" ON "Slots" ("PsychologistId", "StartUtc");
            """);
    }
}
