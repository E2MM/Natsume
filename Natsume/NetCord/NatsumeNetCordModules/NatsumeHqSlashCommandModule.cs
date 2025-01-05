using System.Text;
using Natsume.LiteDB;
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
    public class NatsumeContactsModule(LiteDbService liteDbService)
        : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand(name: "list", description: "Elenca tutti i contatti di Natsume-san")]
        public async Task ListAllContacts()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
            var sb = new StringBuilder(1024);

            foreach (var c in liteDbService.GetAllNatsumeContacts().OrderByDescending(x => x.MaximumFriendship))
            {
                var status = c switch
                {
                    { IsNatsumeFriend: false } => "💔",
                    _ when c.LastMessageOn >= DateTime.Now.AddDays(-2) => "💝",
                    _ when c.LastMessageOn >= DateTime.Now.AddDays(-5) => "💖",
                    _ when c.LastMessageOn >= DateTime.Now.AddDays(-12) => "🤍",
                    _ when c.LastMessageOn >= DateTime.Now.AddDays(-31) => "❤️‍🩹",
                    _ => "💜"
                };

                sb.Append($"🆔 {c.Nickname}\t");
                sb.Append($"{status}\t");
                sb.Append($"🌟 {100 * c.CurrentFriendship:N2}/{100 * c.MaximumFriendship:N2}\t");
                sb.Append("( ");
                sb.Append($"💬 {100 * c.MessageFriendship:N2}\t ⌛ {100 * c.TimeFriendship:N2}\t 🏆 {100 * c.ActivityFriendship:N2}");
                sb.Append(" )\t");
                sb.Append($"#️⃣{c.MessageCount}");
                sb.Append('\n');
            }

            var response = sb.ToString();
            await ModifyResponseAsync(m => m.WithContent(response));
        }
    }
}