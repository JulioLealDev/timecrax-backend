using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Data;

namespace Timecrax.Api.Data.Seed;

public class DbSeedHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly IWebHostEnvironment _env;

    public DbSeedHostedService(IServiceProvider sp, IWebHostEnvironment env)
    {
        _sp = sp;
        _env = env;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Recomendado: rodar apenas em Development (você pode ajustar)
        if (!_env.IsDevelopment())
            return;

        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Garante migrations aplicadas (opcional, mas útil em DEV)
        await db.Database.MigrateAsync(cancellationToken);

        await Seed.SafeRunAsync(db, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static class Seed
    {
        public static async Task SafeRunAsync(AppDbContext db, CancellationToken ct)
        {
            // Evita estourar a app por seed
            try
            {
                await Timecrax.Api.Data.Seed.DbSeeder.SeedAsync(db, ct);
            }
            catch (Exception ex)
            {
                // Em dev, normalmente você quer ver isso no console
                Console.WriteLine($"[SEED] Failed: {ex}");
                throw;
            }
        }
    }
}
