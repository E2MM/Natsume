using Coravel.Invocable;
using Natsume.Services;
using NetCord.Rest;

namespace Natsume.Coravel;

public class RemindMeInvocable(NatsumeDbService natsumeDbService, RestClient client) : IInvocable
{
    public async Task Invoke()
    {
        try
        {
            var reminders = natsumeDbService.GetAllExpiredNatsumeReminders();

            foreach (var reminder in reminders)
            {
                var originalMessage =
                    await client.GetMessageAsync(reminder.DiscordChannelId, reminder.DiscordMessageId);
                if (reminder.ReminderText is "")
                    await originalMessage.ReplyAsync(
                        $"Mi avevi chiesto che ti avvisassi in questo momento!");
                else
                    await originalMessage.ReplyAsync(
                        $"Mi avevi chiesto che ti ricordassi in questo momento \"{reminder.ReminderText}\"!");
            }

            await natsumeDbService.RemoveNatsumeReminders(reminders);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}