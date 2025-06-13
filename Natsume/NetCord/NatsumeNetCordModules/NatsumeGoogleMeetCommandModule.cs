using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

public class NatsumeGoogleMeetCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("meet", "Creates a new Meet on Google Meet",
        Contexts =
        [
            InteractionContextType.Guild,
            InteractionContextType.BotDMChannel,
            InteractionContextType.DMChannel
        ])]
    public async Task SendNewMeetLink(string meetingName = "")
    {
        meetingName = string.IsNullOrWhiteSpace(meetingName)
            ? Guid.NewGuid().ToString()
            : new string(meetingName.AsEnumerable().Where(x => x < 127 && x != ' ').ToArray());

        await RespondAsync(InteractionCallback.Message($"https://g.co/meet/{meetingName}"));
    }
}