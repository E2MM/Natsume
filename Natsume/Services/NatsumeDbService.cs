using Natsume.Database;
using Natsume.Database.Entities;

namespace Natsume.Services;

public class NatsumeDbService(NatsumeDbContext context)
{
    public List<NatsumeContact> GetAllNatsumeContacts()
    {
        return context.Contacts.ToList();
    }

    public NatsumeContact? AddNatsumeContact(NatsumeContact contact)
    {
        context.Contacts.Add(contact);
        context.SaveChanges();
        return GetNatsumeContactById(contact.DiscordId);
    }

    public NatsumeContact? GetNatsumeContactById(ulong discordId)
    {
        return context.Contacts.FirstOrDefault(x => x.DiscordId == discordId);
    }

    public bool UpdateNatsumeContact(NatsumeContact contact)
    {
        context.Contacts.Update(contact);
        return context.SaveChanges() > 0;
    }

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
}