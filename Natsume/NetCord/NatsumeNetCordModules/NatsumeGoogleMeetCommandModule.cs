using System.Text;
using Natsume.Persistence.Meeting;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

internal class NatsumeGoogleMeetCommandModule(NatsumeMeetingService natsumeMeetingService)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [
        SlashCommand(
            name: "meet",
            description: "Creates a new Meet on Google Meet",
            Contexts =
            [
                InteractionContextType.Guild,
                InteractionContextType.BotDMChannel,
                InteractionContextType.DMChannel
            ]
        )
    ]
    public async Task SendNewMeetLink(
        [
            SlashCommandParameter(
                Name = "nome",
                Description = "indica il nome con cui il meeting verrà creato",
                MaxLength = 32)
        ]
        string meetingName = ""
    )
    {
        var sanitizedMeetingName = SanitizeMeetingName(meetingName);
        var isRandomMeeting = string.IsNullOrWhiteSpace(sanitizedMeetingName);

        try
        {
            if (isRandomMeeting)
            {
                var meetingCount = await natsumeMeetingService.CountMeetingsAsNoTrackingAsync();
                sanitizedMeetingName = $"random-meeting-numero-{meetingCount + 1}";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        await RespondAsync(InteractionCallback.Message($"https://g.co/meet/{sanitizedMeetingName}"));

        var newMeeting = new NatsumeMeeting
        (
            meetingName: sanitizedMeetingName,
            discordUserId: Context.User.Id,
            isRandomMeeting: isRandomMeeting
        );
        
        await natsumeMeetingService.AddMeetingAsync(meeting: newMeeting);
    }

    private static string SanitizeMeetingName(string meetingName)
    {
        var sb = new StringBuilder(capacity: meetingName.Length);
        foreach (var c in meetingName.Trim())
        {
            sb.Append(char.IsAsciiLetterOrDigit(c) ? c : '-');
        }
        
        return sb.ToString();
    }
}