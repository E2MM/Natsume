using Microsoft.Extensions.DependencyInjection;
using Natsume.Coravel.InvocableServices;

namespace Natsume.Coravel;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddInvocableServices(this IServiceCollection services)
    {
        return services
            .AddScoped<BondUpInvocable>()
            .AddScoped<RemindMeInvocable>();
    }
}