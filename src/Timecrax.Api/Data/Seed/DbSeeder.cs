using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Timecrax.Api.Data;
using Timecrax.Api.Domain.Entities;
using Timecrax.Api.Domain.Enums;
using Timecrax.Api.Services;

namespace Timecrax.Api.Data.Seed;

public static class DbSeeder
{
    private static readonly Random Random = new();

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // 1) Achievements e Medals (sempre)
        await SeedAchievementsAsync(db, ct);
        await SeedMedalsAsync(db, ct);

        // 2) Test Data (apenas se não houver usuários)
        if (!await db.Users.AnyAsync(ct))
        {
            await SeedTestDataAsync(db, logger, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedTestDataAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        logger.LogInformation("Starting test data seed...");

        // Criar 75 usuários
        var users = CreateTestUsers(75);
        db.Users.AddRange(users);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created {Count} test users", 75);

        // Criar 20 temas completos (15 cartas cada)
        var themes = CreateTestThemes(20, users);
        db.Themes.AddRange(themes);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created {Count} test themes", 20);

        // Criar cartas para cada tema
        foreach (var theme in themes)
        {
            var cards = CreateTestEventCards(theme, 15);
            db.EventCards.AddRange(cards);
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created event cards with quizzes");
        logger.LogInformation("Test data seed completed");
    }

    private static async Task SeedMedalsAsync(AppDbContext db, CancellationToken ct)
    {
        // Se já existe ao menos 1, não replique (idempotente)
        var any = await db.Medals.AnyAsync(ct);
        if (any) return;

        var now = DateTimeOffset.UtcNow;

        var list = new List<Medal>
        {
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz",
                Image = "/assets/medals/medal_01.png",
                MinScore = 0,
                Language = "pt-br",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Bacharel",
                Image = "/assets/medals/medal_02.png",
                MinScore = 1000,
                Language = "pt-br",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Mestre",
                Image = "/assets/medals/medal_03.png",
                MinScore = 2000,
                Language = "pt-br",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Doutor",
                Image = "/assets/medals/medal_04.png",
                MinScore = 3000,
                Language = "pt-br",
                CreatedAt = now
            },
                        new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz",
                Image = "/assets/medals/medal_01.png",
                MinScore = 0,
                Language = "pt-pt",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Licenciado",
                Image = "/assets/medals/medal_02.png",
                MinScore = 1000,
                Language = "pt-pt",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Mestre",
                Image = "/assets/medals/medal_03.png",
                MinScore = 2000,
                Language = "pt-pt",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Doutor",
                Image = "/assets/medals/medal_04.png",
                MinScore = 3000,
                Language = "pt-pt",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz",
                Image = "/assets/medals/medal_01.png",
                MinScore = 0,
                Language = "es",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Graduado",
                Image = "/assets/medals/medal_02.png",
                MinScore = 1000,
                Language = "es",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Máster",
                Image = "/assets/medals/medal_03.png",
                MinScore = 2000,
                Language = "es",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Doctor",
                Image = "/assets/medals/medal_04.png",
                MinScore = 3000,
                Language = "es",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Apprentice",
                Image = "/assets/medals/medal_01.png",
                MinScore = 0,
                Language = "en",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Bachelor",
                Image = "/assets/medals/medal_02.png",
                MinScore = 1000,
                Language = "en",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Master",
                Image = "/assets/medals/medal_03.png",
                MinScore = 2000,
                Language = "en",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Doctor",
                Image = "/assets/medals/medal_04.png",
                MinScore = 3000,
                Language = "en",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Apprenti",
                Image = "/assets/medals/medal_01.png",
                MinScore = 0,
                Language = "fr",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Licence",
                Image = "/assets/medals/medal_02.png",
                MinScore = 1000,
                Language = "fr",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Master",
                Image = "/assets/medals/medal_03.png",
                MinScore = 2000,
                Language = "fr",
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Docteur",
                Image = "/assets/medals/medal_04.png",
                MinScore = 3000,
                Language = "fr",
                CreatedAt = now
            },
        };

        db.Medals.AddRange(list);
    }

    private static async Task SeedAchievementsAsync(AppDbContext db, CancellationToken ct)
    {
        // Se já existe ao menos 1, não replique (idempotente)
        var any = await db.Achievements.AnyAsync(ct);
        if (any) return;

        var now = DateTimeOffset.UtcNow;

        var list = new List<Achievement>
        {
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Primeira Vitória",
                Image = "/assets/achievements/ach_1.png",
                Description = "Complete um tema pela primeira vez.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz Persistente",
                Image = "/assets/achievements/ach_2.png",
                Description = "Complete 5 temas diferentes.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mestre do Passado",
                Image = "/assets/achievements/ach_3.png",
                Description = "Complete 10 temas diferentes.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Expert da História",
                Image = "/assets/achievements/ach_4.png",
                Description = "Complete 20 temas diferentes.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Senhor do Tempo",
                Image = "/assets/achievements/ach_5.png",
                Description = "Complete 50 temas diferentes.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Memória Afiada",
                Image = "/assets/achievements/ach_6.png",
                Description = "Acerte 3 quizzes seguidos.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mente Magnífica",
                Image = "/assets/achievements/ach_7.png",
                Description = "Acerte 5 quizzes seguidos.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Enciclopédia Humana",
                Image = "/assets/achievements/ach_8.png",
                Description = "Acerte 10 quizzes seguidos.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Sorte de Principiante",
                Image = "/assets/achievements/ach_9.png",
                Description = "Complete 1 tema jogando sozinho.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Guerreiro Solitário",
                Image = "/assets/achievements/ach_10.png",
                Description = "Complete 5 temas jogando sozinho.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Exército de um homem só",
                Image = "/assets/achievements/ach_11.png",
                Description = "Complete 10 temas jogando sozinho.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Juntos venceremos",
                Image = "/assets/achievements/ach_12.png",
                Description = "Complete 1 tema jogando em grupo.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "A união faz a força!",
                Image = "/assets/achievements/ach_13.png",
                Description = "Complete 5 temas jogando em grupo.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Legião do saber",
                Image = "/assets/achievements/ach_14.png",
                Description = "Complete 10 temas jogando em grupo.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Reis da História",
                Image = "/assets/achievements/ach_15.png",
                Description = "Complete 20 temas jogando em grupo.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Ancioês do Tempo",
                Image = "/assets/achievements/ach_16.png",
                Description = "Complete 50 temas jogando em grupo.",
                Language = "pt-br",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Primeira Vitória",
                Image = "/assets/achievements/ach_1.png",
                Description = "Completa um tema pela primeira vez.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz Persistente",
                Image = "/assets/achievements/ach_2.png",
                Description = "Completa 5 temas diferentes.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mestre do Passado",
                Image = "/assets/achievements/ach_3.png",
                Description = "Completa 10 temas diferentes.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Especialista em História",
                Image = "/assets/achievements/ach_4.png",
                Description = "Completa 20 temas diferentes.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Senhor do Tempo",
                Image = "/assets/achievements/ach_5.png",
                Description = "Completa 50 temas diferentes.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Memória Afiada",
                Image = "/assets/achievements/ach_6.png",
                Description = "Acerta 3 quizzes seguidos.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mente Magnífica",
                Image = "/assets/achievements/ach_7.png",
                Description = "Acerta 5 quizzes seguidos.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Enciclopédia Humana",
                Image = "/assets/achievements/ach_8.png",
                Description = "Acerta 10 quizzes seguidos.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Sorte de Principiante",
                Image = "/assets/achievements/ach_9.png",
                Description = "Completa 1 tema jogando sozinho.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Guerreiro Solitário",
                Image = "/assets/achievements/ach_10.png",
                Description = "Completa 5 temas jogando sozinho.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Exército de um homem só",
                Image = "/assets/achievements/ach_11.png",
                Description = "Completa 10 temas jogando sozinho.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Juntos venceremos",
                Image = "/assets/achievements/ach_12.png",
                Description = "Completa 1 tema jogando em grupo.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "A união faz a força!",
                Image = "/assets/achievements/ach_13.png",
                Description = "Completa 5 temas jogando em grupo.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Legião do saber",
                Image = "/assets/achievements/ach_14.png",
                Description = "Completa 10 temas jogando em grupo.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Reis da História",
                Image = "/assets/achievements/ach_15.png",
                Description = "Completa 20 temas jogando em grupo.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Ancioês do Tempo",
                Image = "/assets/achievements/ach_16.png",
                Description = "Completa 50 temas jogando em grupo.",
                Language = "pt-pt",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Primera Victoria",
                Image = "/assets/achievements/ach_1.png",
                Description = "Completa un tema por primera vez.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz Persistente",
                Image = "/assets/achievements/ach_2.png",
                Description = "Completa 5 temas diferentes.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Maestro del Pasado",
                Image = "/assets/achievements/ach_3.png",
                Description = "Completa 10 temas diferentes.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Experto en Historia",
                Image = "/assets/achievements/ach_4.png",
                Description = "Completa 20 temas diferentes.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Señor del Tiempo",
                Image = "/assets/achievements/ach_5.png",
                Description = "Completa 50 temas diferentes.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Memoria Afilada",
                Image = "/assets/achievements/ach_6.png",
                Description = "Acierta 3 cuestionarios seguidos.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mente Magnífica",
                Image = "/assets/achievements/ach_7.png",
                Description = "Acierta 5 cuestionarios seguidos.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Enciclopedia Humana",
                Image = "/assets/achievements/ach_8.png",
                Description = "Acierta 10 cuestionarios seguidos.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Suerte de Principiante",
                Image = "/assets/achievements/ach_9.png",
                Description = "Completa 1 tema jugando solo.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Guerrero Solitario",
                Image = "/assets/achievements/ach_10.png",
                Description = "Completa 5 temas jugando solo.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Un Ejército de Un Solo Hombre",
                Image = "/assets/achievements/ach_11.png",
                Description = "Completa 10 temas jugando solo.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Juntos Venceremos",
                Image = "/assets/achievements/ach_12.png",
                Description = "Completa 1 tema jugando en grupo.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "La Unión Hace la Fuerza",
                Image = "/assets/achievements/ach_13.png",
                Description = "Completa 5 temas jugando en grupo.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Legión del Saber",
                Image = "/assets/achievements/ach_14.png",
                Description = "Completa 10 temas jugando en grupo.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Reyes de la Historia",
                Image = "/assets/achievements/ach_15.png",
                Description = "Completa 20 temas jugando en grupo.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Ancianos del Tiempo",
                Image = "/assets/achievements/ach_16.png",
                Description = "Completa 50 temas jugando en grupo.",
                Language = "es",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "First Victory",
                Image = "/assets/achievements/ach_1.png",
                Description = "Complete a topic for the first time.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Persistent Apprentice",
                Image = "/assets/achievements/ach_2.png",
                Description = "Complete 5 different topics.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Master of the Past",
                Image = "/assets/achievements/ach_3.png",
                Description = "Complete 10 different topics.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "History Expert",
                Image = "/assets/achievements/ach_4.png",
                Description = "Complete 20 different topics.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Lord of Time",
                Image = "/assets/achievements/ach_5.png",
                Description = "Complete 50 different topics.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Sharp Memory",
                Image = "/assets/achievements/ach_6.png",
                Description = "Get 3 quizzes right in a row.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Magnificent Mind",
                Image = "/assets/achievements/ach_7.png",
                Description = "Get 5 quizzes right in a row.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Human Encyclopedia",
                Image = "/assets/achievements/ach_8.png",
                Description = "Get 10 quizzes right in a row.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Beginner’s Luck",
                Image = "/assets/achievements/ach_9.png",
                Description = "Complete 1 topic playing solo.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Lone Warrior",
                Image = "/assets/achievements/ach_10.png",
                Description = "Complete 5 topics playing solo.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "One-Man Army",
                Image = "/assets/achievements/ach_11.png",
                Description = "Complete 10 topics playing solo.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Together We Will Win",
                Image = "/assets/achievements/ach_12.png",
                Description = "Complete 1 topic playing in a group.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Strength in Unity",
                Image = "/assets/achievements/ach_13.png",
                Description = "Complete 5 topics playing in a group.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Legion of Knowledge",
                Image = "/assets/achievements/ach_14.png",
                Description = "Complete 10 topics playing in a group.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Kings of History",
                Image = "/assets/achievements/ach_15.png",
                Description = "Complete 20 topics playing in a group.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Elders of Time",
                Image = "/assets/achievements/ach_16.png",
                Description = "Complete 50 topics playing in a group.",
                Language = "en",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Première Victoire",
                Image = "/assets/achievements/ach_1.png",
                Description = "Complétez un thème pour la première fois.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Apprenti Persévérant",
                Image = "/assets/achievements/ach_2.png",
                Description = "Complétez 5 thèmes différents.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Maître du Passé",
                Image = "/assets/achievements/ach_3.png",
                Description = "Complétez 10 thèmes différents.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Expert en Histoire",
                Image = "/assets/achievements/ach_4.png",
                Description = "Complétez 20 thèmes différents.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Seigneur du Temps",
                Image = "/assets/achievements/ach_5.png",
                Description = "Complétez 50 thèmes différents.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mémoire Affûtée",
                Image = "/assets/achievements/ach_6.png",
                Description = "Réussissez 3 quiz consécutifs.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Esprit Magnifique",
                Image = "/assets/achievements/ach_7.png",
                Description = "Réussissez 5 quiz consécutifs.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Encyclopédie Humaine",
                Image = "/assets/achievements/ach_8.png",
                Description = "Réussissez 10 quiz consécutifs.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Chance du Débutant",
                Image = "/assets/achievements/ach_9.png",
                Description = "Complétez 1 thème en jouant seul.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Guerrier Solitaire",
                Image = "/assets/achievements/ach_10.png",
                Description = "Complétez 5 thèmes en jouant seul.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Armée d’un Seul Homme",
                Image = "/assets/achievements/ach_11.png",
                Description = "Complétez 10 thèmes en jouant seul.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Ensemble, Nous Vaincrons",
                Image = "/assets/achievements/ach_12.png",
                Description = "Complétez 1 thème en jouant en groupe.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "L’Union Fait la Force",
                Image = "/assets/achievements/ach_13.png",
                Description = "Complétez 5 thèmes en jouant en groupe.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Légion du Savoir",
                Image = "/assets/achievements/ach_14.png",
                Description = "Complétez 10 thèmes en jouant en groupe.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Rois de l’Histoire",
                Image = "/assets/achievements/ach_15.png",
                Description = "Complétez 20 thèmes en jouant en groupe.",
                Language = "fr",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Anciens du Temps",
                Image = "/assets/achievements/ach_16.png",
                Description = "Complétez 50 thèmes en jouant en groupe.",
                Language = "fr",
                CreatedAt = now
            },
        };

        db.Achievements.AddRange(list);
    }

    // ========== TEST DATA HELPERS ==========

    private static readonly string[] FirstNames =
    {
        "João", "Maria", "Pedro", "Ana", "Lucas", "Julia", "Gabriel", "Beatriz",
        "Rafael", "Larissa", "Felipe", "Camila", "Bruno", "Amanda", "Thiago", "Carolina",
        "Diego", "Fernanda", "Matheus", "Juliana", "Rodrigo", "Mariana", "Vitor", "Patricia",
        "Carlos", "Aline", "Daniel", "Leticia", "Fernando", "Gabriela", "Gustavo", "Bruna",
        "Marcelo", "Renata", "Leonardo", "Tatiane", "Ricardo", "Vanessa", "André", "Cristina",
        "Paulo", "Sandra", "Roberto", "Monica", "José", "Claudia", "Antonio", "Silvia",
        "Marcos", "Luciana", "Eduardo", "Adriana", "Alexandre", "Rosangela", "Fabio", "Regina"
    };

    private static readonly string[] LastNames =
    {
        "Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira",
        "Lima", "Gomes", "Costa", "Ribeiro", "Martins", "Carvalho", "Rocha", "Almeida",
        "Nascimento", "Barbosa", "Cardoso", "Dias", "Fernandes", "Araujo", "Castro", "Cavalcante"
    };

    private static readonly string[] ThemeNames =
    {
        "Revolução Francesa",
        "Segunda Guerra Mundial",
        "Renascimento Italiano",
        "Império Romano",
        "Descobrimento do Brasil",
        "Revolução Industrial",
        "Guerra Fria",
        "Idade Média",
        "Civilização Egípcia",
        "Grécia Antiga",
        "Independência Americana",
        "Era Napoleônica",
        "Primeira Guerra Mundial",
        "Revolução Russa",
        "Expansão Marítima",
        "Reforma Protestante",
        "Iluminismo",
        "Absolutismo Europeu",
        "Feudalismo",
        "Civilizações Pré-Colombianas"
    };

    private static readonly string[] ThemeResumes =
    {
        "Eventos que transformaram a França e inspiraram revoluções pelo mundo.",
        "O maior conflito da história que mudou o curso da humanidade.",
        "O renascimento das artes, ciências e humanismo na Itália.",
        "O império que dominou o mundo antigo por séculos.",
        "A chegada dos portugueses às terras brasileiras em 1500.",
        "A transformação da produção e da sociedade através das máquinas.",
        "A tensão entre EUA e URSS que dividiu o mundo em dois blocos.",
        "Mil anos de história europeia entre Roma e o Renascimento.",
        "A civilização que construiu as pirâmides e dominou o Nilo.",
        "Berço da democracia, filosofia e cultura ocidental.",
        "As 13 colônias se libertam do domínio britânico.",
        "A ascensão e queda do imperador que conquistou a Europa.",
        "A Grande Guerra que devastou a Europa e mudou fronteiras.",
        "A queda dos czares e o nascimento da União Soviética.",
        "Portugueses e espanhóis exploram os oceanos e descobrem novos mundos.",
        "Martinho Lutero e a divisão do cristianismo.",
        "O século das luzes que valorizou a razão e a ciência.",
        "Reis absolutos concentram todo o poder nas monarquias europeias.",
        "Sistema político e social baseado em vassalagem e terras.",
        "Astecas, Maias e Incas: grandes civilizações da América pré-colonial."
    };

    private static readonly string[] EventTitles =
    {
        "Batalha Decisiva", "Tratado de Paz", "Descoberta Importante", "Revolução Popular",
        "Coroação Imperial", "Queda do Império", "Invenção Revolucionária", "Conquista Territorial",
        "Reforma Política", "Construção Monumental", "Declaração Histórica", "Aliança Estratégica",
        "Revolução Cultural", "Expedição Marítima", "Fundação da Cidade"
    };

    private static List<User> CreateTestUsers(int count)
    {
        var users = new List<User>();
        var usedEmails = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            string email;
            string firstName = FirstNames[Random.Next(FirstNames.Length)];
            string lastName = LastNames[Random.Next(LastNames.Length)];

            do
            {
                email = $"{firstName.ToLower()}.{lastName.ToLower()}{Random.Next(1, 999)}@test.com";
            } while (usedEmails.Contains(email));

            usedEmails.Add(email);

            var role = Random.Next(10) < 3 ? "teacher" : "student"; // 30% teachers, 70% students

            users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = PasswordService.Hash("Test123!"),
                Role = role,
                Score = Random.Next(800, 3201), // Entre 800 e 3200
                SchoolName = role == "teacher" ? $"Escola {Random.Next(1, 21)}" : null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Next(1, 365)),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Next(0, 30))
            });
        }

        return users;
    }

    private static readonly string[] Recommendations =
    {
        "1º cicle: 6 - 10 years old",
        "2º cicle: 10 - 12 years old",
        "3º cicle: 12 - 15 years old",
        "4º cicle: 15 - 18 years old"
    };

    private static List<Theme> CreateTestThemes(int count, List<User> users)
    {
        var themes = new List<Theme>();
        var teachers = users.Where(u => u.Role == "teacher").ToList();

        // Se não houver teachers, usar alguns users aleatórios
        if (!teachers.Any())
        {
            teachers = users.Take(5).ToList();
        }

        for (int i = 0; i < count; i++)
        {
            var creator = teachers[Random.Next(teachers.Count)];

            themes.Add(new Theme
            {
                Id = Guid.NewGuid(),
                Name = ThemeNames[i],
                Resume = ThemeResumes[i],
                Recommendation = Recommendations[Random.Next(Recommendations.Length)],
                Image = $"https://picsum.photos/seed/theme{i}/400/300",
                ReadyToPlay = true, // Todos prontos para jogar
                CreatorUserId = creator.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Next(30, 180)),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Next(0, 30))
            });
        }

        return themes;
    }

    private static List<EventCard> CreateTestEventCards(Theme theme, int count)
    {
        var cards = new List<EventCard>();

        for (int i = 0; i < count; i++)
        {
            var year = Random.Next(500, 2000);
            var era = year < 1000 ? Era.BC : Era.AD;

            var card = new EventCard
            {
                Id = Guid.NewGuid(),
                ThemeId = theme.Id,
                Title = $"{EventTitles[Random.Next(EventTitles.Length)]} {i + 1}",
                Image = $"https://picsum.photos/seed/card{theme.Id}{i}/600/400",
                Year = year < 1000 ? 1000 - year : year,
                Era = era,
                OrderIndex = i,
                CreatedAt = theme.CreatedAt.AddHours(i),

                // Image Quiz
                ImageQuiz = new ImageQuiz
                {
                    Id = Guid.NewGuid(),
                    Question = "Qual imagem representa melhor este evento histórico?",
                    Image1 = $"https://picsum.photos/seed/img1{theme.Id}{i}/300/200",
                    Image2 = $"https://picsum.photos/seed/img2{theme.Id}{i}/300/200",
                    Image3 = $"https://picsum.photos/seed/img3{theme.Id}{i}/300/200",
                    Image4 = $"https://picsum.photos/seed/img4{theme.Id}{i}/300/200",
                    CorrectImageIndex = (short)Random.Next(0, 4)
                },

                // Text Quiz
                TextQuiz = new TextQuiz
                {
                    Id = Guid.NewGuid(),
                    Question = "Qual foi a principal consequência deste evento?",
                    Text1 = "Mudança política significativa",
                    Text2 = "Revolução tecnológica",
                    Text3 = "Expansão territorial",
                    Text4 = "Reforma social",
                    CorrectTextIndex = (short)Random.Next(0, 4)
                },

                // True or False Quiz
                TrueOrFalseQuiz = new TrueOrFalseQuiz
                {
                    Id = Guid.NewGuid(),
                    Text = "Este evento marcou o início de uma nova era histórica.",
                    IsTrue = Random.Next(2) == 1
                },

                // Correlation Quiz
                CorrelationQuiz = new CorrelationQuiz
                {
                    Id = Guid.NewGuid(),
                    Image1 = $"https://picsum.photos/seed/cor1{theme.Id}{i}/250/200",
                    Image2 = $"https://picsum.photos/seed/cor2{theme.Id}{i}/250/200",
                    Image3 = $"https://picsum.photos/seed/cor3{theme.Id}{i}/250/200",
                    Text1 = "Líder histórico do período",
                    Text2 = "Local onde ocorreu o evento",
                    Text3 = "Documento importante relacionado"
                }
            };

            cards.Add(card);
        }

        return cards;
    }
}
