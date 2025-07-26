using Microsoft.EntityFrameworkCore;
using Natsume.Database;
using Natsume.Database.Entities;

namespace Natsume.Services;

public class NatsumeDbService(NatsumeDbContext context)
{
    public NatsumeContact? GetNatsumeReminderById(ulong discordId)
    {
        return context.Contacts.FirstOrDefault(x => x.DiscordId == discordId);
    }

    public async Task AddNatsumeReminder(NatsumeReminder reminder)
    {
        context.Reminders.Add(reminder);
        await context.SaveChangesAsync();
    }

    public List<NatsumeReminder> GetAllExpiredNatsumeReminders()
    {
        return context.Reminders
            .Where(x => x.RemindMeAt <= DateTime.Now)
            .ToList();
    }

    public async Task RemoveNatsumeReminders(List<NatsumeReminder> reminders)
    {
        context.Reminders.RemoveRange(reminders);
        await context.SaveChangesAsync();
    }

    public Task<int> GetMeetingCountAsync(CancellationToken token = default)
    {
        return context
            .Meetings
            .AsNoTracking()
            .CountAsync(
                predicate: m => m.IsRandomMeeting,
                cancellationToken: token
            );
    }

    internal Task<int> AddMeetingAsync(NatsumeMeeting meeting, CancellationToken token = default)
    {
        context.Meetings.Add(entity: meeting);
        return context.SaveChangesAsync();
    }
}