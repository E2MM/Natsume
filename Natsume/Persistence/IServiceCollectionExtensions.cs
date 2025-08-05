using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Natsume.Persistence.Contact;
using Natsume.Persistence.Meeting;
using Natsume.Persistence.Reminder;

namespace Natsume.Persistence;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, string sqliteConnection)
    {
        return services
            .AddDbContext<NatsumeDbContext>(options => options.UseSqlite(sqliteConnection))
            .AddScoped<NatsumeReminderService>()
            .AddScoped<NatsumeMeetingService>()
            .AddScoped<NatsumeContactService>();
    }
}