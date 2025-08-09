using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Natsume.Persistence;

public static class IServiceProviderExtensions
{
    public static async Task MigrateDatabaseAsync(this IHost host, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        await using var scope = host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NatsumeDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken: token);
    }
}