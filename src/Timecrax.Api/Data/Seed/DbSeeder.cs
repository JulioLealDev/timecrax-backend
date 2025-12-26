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

        // 2) (Opcional) Themes iniciais (se você quiser seedar um theme "demo")
        // await SeedDemoThemesAsync(db, ct);

        await db.SaveChangesAsync(ct);
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
                Image = "https://example.com/achievements/first-win.png",
                Description = "Complete um tema pela primeira vez.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz Persistente",
                Image = "https://example.com/achievements/persistent.png",
                Description = "Complete 5 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mestre da História",
                Image = "https://example.com/achievements/master.png",
                Description = "Complete 20 temas diferentes.",
                CreatedAt = now
            }
        };

        db.Achievements.AddRange(list);
    }
}
