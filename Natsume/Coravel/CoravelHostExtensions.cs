using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.Extensions.Hosting;
using Natsume.Coravel.InvocableServices;

namespace Natsume.Coravel;

public static class CoravelHostExtensions
{
    public static ISchedulerConfiguration UseCoravelScheduledInvocableServices(this IHost host)
    {
        return host.Services
            .UseScheduler(ScheduleInvocableServices)
            .OnError(e => Console.WriteLine(e.Message));

        void ScheduleInvocableServices(IScheduler scheduler)
        {
            scheduler.Schedule<BondUpInvocable>()
                .Hourly();

            scheduler.Schedule<RemindMeInvocable>()
                .EveryFiveMinutes();

            scheduler.Schedule<DailyScrumInvocable>()
                .Cron("31 10 * * 1-5")
                .Zoned(TimeZoneInfo.Local);
        }
    }
}