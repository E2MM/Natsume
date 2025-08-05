using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Natsume.Coravel.InvocableServices;

namespace Natsume.Coravel;

public static class IServiceProviderExtensions
{
    public static ISchedulerConfiguration UseScheduledInvocableServices(this IServiceProvider services)
    {
        return services.UseScheduler(scheduler =>
            {
                scheduler
                    .Schedule<BondUpInvocable>()
                    .Hourly();

                scheduler
                    .Schedule<RemindMeInvocable>()
                    .EveryFiveMinutes();

                scheduler
                    .Schedule<DailyScrumInvocable>()
                    .DailyAt(hour: 10, minute: 5);
            }
        );
    }
}