using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Natsume.Coravel;
using Natsume.Database;
using Natsume.Database.Services;
using Natsume.Services;

namespace Natsume.Utils;

public static class HostApplicationBuilderExtensions
{
    public static IServiceCollection AddInvocableServices(this HostApplicationBuilder builder)
    {
        return builder
            .Services
            .AddScoped<BondUpInvocable>()
            .AddScoped<RemindMeInvocable>();
    }

    public static IServiceCollection AddDbServices(this HostApplicationBuilder builder, string sqliteConnection)
    {
        return builder
            .Services
            .AddDbContext<NatsumeDbContext>(options => options.UseSqlite(sqliteConnection))
            .AddScoped<NatsumeDbService>()
            .AddScoped<NatsumeContactService>();
    }
}