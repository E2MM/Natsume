using System.ComponentModel.DataAnnotations;
using Natsume.Persistence.Reminder;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

internal class NatsumeRemindMeCommandModule(NatsumeReminderService natsumeDbService)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [
        SlashCommand(
            name: "remindme",
            description: "Ricordami qualcosa fra un po' di tempo!",
            Contexts =
            [
                InteractionContextType.Guild,
                InteractionContextType.BotDMChannel,
                InteractionContextType.DMChannel
            ]
        )
    ]
    public async Task RemindMe(
        [SlashCommandParameter(Name = "promemoria", Description = "testo del promemoria"), MaxLength(512)]
        string reminderText = "",
        [SlashCommandParameter(Name = "giorni", Description = "fra quanti giorni", MinValue = 0)]
        int days = 0,
        [SlashCommandParameter(Name = "ore", Description = "fra quante ore", MinValue = 0)]
        int hours = 0,
        [SlashCommandParameter(Name = "minuti", Description = "fra quanti minuti", MinValue = 0)]
        int minutes = 1
    )
    {
        var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));

        var remindMeAt = DateTime.Now
            .AddDays(days)
            .AddHours(hours)
            .AddMinutes(minutes);

        var reminderAnchorMessageContent = new InteractionMessageProperties()
            .WithContent($"Ok, ti manderò un reminder il {remindMeAt:dd/MM/yyyy} alle {remindMeAt:HH:mm}!");

        await RespondAsync(
            callback: InteractionCallback.Message(reminderAnchorMessageContent),
            cancellationToken: cts.Token
        );

        var reminderAnchorMessage = await Context.Interaction.GetResponseAsync(cancellationToken: cts.Token);

        var reminder = new NatsumeReminder(
            discordChannelId: Context.Channel.Id,
            discordMessageId: reminderAnchorMessage.Id,
            discordUserId: Context.User.Id,
            remindMeAt: remindMeAt,
            reminderText: reminderText
        );

        await natsumeDbService.AddNatsumeReminderAsync(reminder, cancellationToken: cts.Token);
    }
}