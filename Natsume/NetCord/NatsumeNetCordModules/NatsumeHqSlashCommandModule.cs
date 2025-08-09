using System.Text;
using Natsume.Persistence.Contact;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

[SlashCommand("hq", "HQ commands",
    DefaultGuildUserPermissions = Permissions.Administrator,
    Contexts = [InteractionContextType.Guild, InteractionContextType.BotDMChannel, InteractionContextType.DMChannel])]
public class NatsumeHqSlashCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("contacts", "Contacts commands")]
    public class NatsumeContactsModule(NatsumeContactService natsumeContactService)
        : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand(name: "list", description: "Elenca tutti i contatti di Natsume-san")]
        public async Task ListAllContacts()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
            var sb = new StringBuilder(1024);

            foreach (var c in (await natsumeContactService.GetAllNatsumeContactsAsNoTrackingAsync()))
            {
                var status = c switch
                {
                    { IsFriend: false } => "💔",
                    _ when c.LastInteraction >= DateTime.Now.AddDays(-2) => "💝",
                    _ when c.LastInteraction >= DateTime.Now.AddDays(-5) => "💖",
                    _ when c.LastInteraction >= DateTime.Now.AddDays(-12) => "🤍",
                    _ when c.LastInteraction >= DateTime.Now.AddDays(-31) => "❤️‍🩹",
                    _ => "💜"
                };

                sb.Append($"🆔 {c.DiscordNickname}\t");
                sb.Append($"{status}\t");
                sb.Append($"💞 {c.Friendship:N2}\t");
                sb.Append($"{c.TotalInteractions} 💌\t");
                sb.Append($"💸 {c.TotalFavorExpended:N2}\t");
                sb.Append("( ");
                sb.Append($" {(DateTime.Now - c.MetOn).TotalDays:N0} 📆 x {c.DailyAverageFavorExpended:N2} ");
                sb.Append(" )\t");
                sb.Append($"🌟 {100 * c.CurrentFavor:N2} / {100 * c.MaximumFavor:N2}\t");
                sb.Append("( ");
                sb.Append($" 💬 {100 * c.MessageFriendship:N2} + ⌛ {100 * c.TimeFriendship:N2} ");
                sb.Append(" )\t");
                sb.Append('\n');
            }

            var response = sb.ToString();
            await ModifyResponseAsync(m => m.WithContent(response));
        }
    }
}