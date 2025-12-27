using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Data;
using Timecrax.Api.Domain.Entities;

namespace Timecrax.Api.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // 1) Achievements
        await SeedAchievementsAsync(db, ct);
        await SeedMedalsAsync(db, ct);

        // 2) (Opcional) Themes iniciais (se você quiser seedar um theme "demo")
        // await SeedDemoThemesAsync(db, ct);

        await db.SaveChangesAsync(ct);
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
                Image = "http://localhost:5139/media/assets/medals/medal_01.png",
                MinScore = 0,
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Bacharel",
                Image = "http://localhost:5139/media/assets/medals/medal_02.png",
                MinScore = 0,
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Mestre",
                Image = "http://localhost:5139/media/assets/medals/medal_03.png",
                MinScore = 0,
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Doutor",
                Image = "http://localhost:5139/media/assets/medals/medal_04.png",
                MinScore = 0,
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
                Image = "http://localhost:5139/media/assets/achievements/ach_1.png",
                Description = "Complete um tema pela primeira vez.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz Persistente",
                Image = "http://localhost:5139/media/assets/achievements/ach_2.png",
                Description = "Complete 5 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mestre do Passado",
                Image = "http://localhost:5139/media/assets/achievements/ach_3.png",
                Description = "Complete 10 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Expert da História",
                Image = "http://localhost:5139/media/assets/achievements/ach_4.png",
                Description = "Complete 20 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Senhor do Tempo",
                Image = "http://localhost:5139/media/assets/achievements/ach_5.png",
                Description = "Complete 50 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Memória Afiada",
                Image = "http://localhost:5139/media/assets/achievements/ach_6.png",
                Description = "Acerte 3 quizzes seguidos.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mente Magnífica",
                Image = "http://localhost:5139/media/assets/achievements/ach_7.png",
                Description = "Acerte 5 quizzes seguidos.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Enciclopédia Humana",
                Image = "http://localhost:5139/media/assets/achievements/ach_8.png",
                Description = "Acerte 10 quizzes seguidos.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Sorte de Principiante",
                Image = "http://localhost:5139/media/assets/achievements/ach_9.png",
                Description = "Complete 1 tema jogando sozinho.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Guerreiro Solitário",
                Image = "http://localhost:5139/media/assets/achievements/ach_10.png",
                Description = "Complete 5 temas jogando sozinho.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Exército de um homem só",
                Image = "http://localhost:5139/media/assets/achievements/ach_11.png",
                Description = "Complete 10 temas jogando sozinho.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Juntos venceremos",
                Image = "http://localhost:5139/media/assets/achievements/ach_12.png",
                Description = "Complete 1 tema jogando em grupo.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "A união faz a força!",
                Image = "http://localhost:5139/media/assets/achievements/ach_13.png",
                Description = "Complete 5 temas jogando em grupo.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Legião do saber",
                Image = "http://localhost:5139/media/assets/achievements/ach_14.png",
                Description = "Complete 10 temas jogando em grupo.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Reis da História",
                Image = "http://localhost:5139/media/assets/achievements/ach_15.png",
                Description = "Complete 20 temas jogando em grupo.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Ancioês do Tempo",
                Image = "http://localhost:5139/media/assets/achievements/ach_16.png",
                Description = "Complete 50 temas jogando em grupo.",
                CreatedAt = now
            }
        };

        db.Achievements.AddRange(list);
    }
}
