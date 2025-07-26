using System.ComponentModel.DataAnnotations;
using Natsume.Database.Entities;
using Natsume.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

internal class NatsumeRemindMeCommandModule(NatsumeDbService natsumeDbService)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand(
        name: "remindme",
        description: "Ricordami qualcosa fra un po' di tempo!",
        Contexts =
        [
            InteractionContextType.Guild,
            InteractionContextType.BotDMChannel,
            InteractionContextType.DMChannel
        ]
    )]
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
        var remindMeAt = DateTime.Now
            .AddDays(days)
            .AddHours(hours)
            .AddMinutes(minutes);

        var reminderAnchorMessageContent = new InteractionMessageProperties()
            .WithContent($"Ok, ti manderò un reminder il {remindMeAt:dd/MM/yyyy} alle {remindMeAt:HH:mm}!");

        await RespondAsync(InteractionCallback.Message(reminderAnchorMessageContent));

        var reminderAnchorMessage = await Context.Interaction.GetResponseAsync();

        NatsumeReminder reminder = new(
            discordChannelId: Context.Channel.Id,
            discordMessageId: reminderAnchorMessage.Id,
            discordUserId: Context.User.Id,
            remindMeAt: remindMeAt,
            reminderText: reminderText
        );

        await natsumeDbService.AddNatsumeReminder(reminder);
    }
}