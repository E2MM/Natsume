using Coravel;
using Microsoft.Extensions.DependencyInjection;
using Natsume.Coravel.InvocableServices;

namespace Natsume.Coravel;

public static class CoravelServiceCollectionExtensions
{
    public static IServiceCollection AddCoravelInvocableServices(
        this IServiceCollection services
    )
    {
        return services
            .AddScheduler()
            .AddScoped<BondUpInvocable>()
            .AddScoped<RemindMeInvocable>()
            .AddScoped<DailyScrumInvocable>();
    }
}