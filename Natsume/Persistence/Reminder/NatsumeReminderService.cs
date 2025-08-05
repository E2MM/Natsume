using Microsoft.EntityFrameworkCore;

namespace Natsume.Persistence.Reminder;

public class NatsumeReminderService(NatsumeDbContext context)
{
    public Task<NatsumeReminder?> GetNatsumeReminderByIdAsync(
        int reminderId,
        CancellationToken cancellationToken = default
    )
    {
        return context.Reminders.FirstOrDefaultAsync(x => x.Id == reminderId, cancellationToken);
    }

    public async Task<NatsumeReminder> AddNatsumeReminderAsync(
        NatsumeReminder reminder,
        CancellationToken cancellationToken = default
    )
    {
        context.Reminders.Add(reminder);
        await context.SaveChangesAsync(cancellationToken);

        return reminder;
    }

    public Task<List<NatsumeReminder>> GetAllExpiredNatsumeRemindersAsync(
        CancellationToken cancellationToken = default
    )
    {
        return context.Reminders
            .Where(x => x.RemindMeAt <= DateTime.Now)
            .ToListAsync(cancellationToken);
    }

    public Task<int> RemoveNatsumeReminderAsync(
        NatsumeReminder reminder,
        CancellationToken cancellationToken = default
    )
    {
        context.Reminders.Remove(reminder);
        return context.SaveChangesAsync(cancellationToken);
    }
}