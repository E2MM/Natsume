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
    [SubSlashCommand("subscribers", "Subscribers commands")]
    public class SubscribersModule(LiteDbService liteDbService) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand(name: "list", description: "Elenca tutti i conoscenti di Natsume-san")]
        public async Task ListSubscribers()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
            var sb = new StringBuilder(1024);

            foreach (var s in liteDbService.GetSubscribers().OrderByDescending(x => x.TotalInvocations))
            {
                var status = s switch
                {
                    { ActiveSubscription: false } => "💔",
                    _ when s.LastInvocation >= DateTime.Now.AddDays(-2) => "💝",
                    _ when s.LastInvocation >= DateTime.Now.AddDays(-5) => "💖",
                    _ when s.LastInvocation >= DateTime.Now.AddDays(-12) => "🤍",
                    _ when s.LastInvocation >= DateTime.Now.AddDays(-31) => "❤️‍🩹",
                    _ => "💜"
                };

                sb.AppendLine($"{status}\t #️⃣ {s.TotalInvocations}\t 🌟 {100 * s.CurrentBalance:N2}\t 🆔 {s.Username}");
            }

            var response = sb.ToString();
            await ModifyResponseAsync(m => m.WithContent(response));
        }
        
    }
}