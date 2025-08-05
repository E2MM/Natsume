using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Natsume.Persistence;

public static class IServiceProviderExtensions
{
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NatsumeDbContext>();
        await db.Database.MigrateAsync(token);
    }
}