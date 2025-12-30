using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Data;

namespace Timecrax.Api.Data.Seed;

public class DbSeedHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DbSeedHostedService> _logger;

    public DbSeedHostedService(IServiceProvider sp, IWebHostEnvironment env, ILogger<DbSeedHostedService> logger)
    {
        _sp = sp;
        _env = env;
        _logger = logger;
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

        try
        {
            await DbSeeder.SeedAsync(db, _logger, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database seed failed");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
