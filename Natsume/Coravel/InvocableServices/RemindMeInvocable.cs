using Coravel.Invocable;
using Natsume.Persistence.Reminder;
using NetCord.Rest;

namespace Natsume.Coravel.InvocableServices;

public class RemindMeInvocable(NatsumeReminderService natsumeDbService, RestClient client)
    : IInvocable, ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }

    public async Task Invoke()
    {
        try
        {
            CancellationToken.ThrowIfCancellationRequested();
            
            var reminders = await natsumeDbService.GetAllExpiredNatsumeRemindersAsync(
                cancellationToken: CancellationToken
            );

            foreach (var reminder in reminders)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var originalMessage = await client.GetMessageAsync(
                    channelId: reminder.DiscordChannelId,
                    messageId: reminder.DiscordMessageId,
                    cancellationToken: CancellationToken
                );

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
                {
                    await originalMessage.ReplyAsync(
                        replyMessage: messageWithoutReminder,
                        cancellationToken: CancellationToken
                    );
                }
                else
                {
                    await originalMessage.ReplyAsync(
                        replyMessage: messageWithReminder,
                        cancellationToken: CancellationToken
                    );
                }

                await natsumeDbService.RemoveNatsumeReminderAsync(
                    reminder: reminder,
                    cancellationToken: CancellationToken
                );
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}