using Coravel.Invocable;
using Natsume.NetCord.NatsumeNetCordModules;
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

                var messageWithoutReminder = new ReplyMessageProperties()
                    .WithContent($"<@{reminder.DiscordUserId}> Mi avevi chiesto che ti avvisassi in questo momento!");

                var messageWithReminder = new ReplyMessageProperties()
                    .WithContent(
                        $"""
                         <@{reminder.DiscordUserId}> Mi avevi chiesto che ti ricordassi in questo momento 
                         > {reminder.ReminderText}
                         """
                    );

                if (reminder.ReminderText is "")
                    await originalMessage.ReplyAsync(messageWithoutReminder);
                else
                    await originalMessage.ReplyAsync(messageWithReminder);
            }

            await natsumeDbService.RemoveNatsumeReminders(reminders);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}