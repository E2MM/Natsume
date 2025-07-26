using Natsume.Database.Services;
using Natsume.NetCord.NatsumeAI;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

public class NatsumeHqUserCommandModule(
    NatsumeContactService natsumeContactService,
    NatsumeAi natsumeAi
) : NatsumeAiCommandModule(natsumeAi)
{
    private readonly NatsumeAi _natsumeAi = natsumeAi;

    [
        UserCommand(
            name: "Presentami a Natsume-san!",
            DefaultGuildUserPermissions = Permissions.Administrator,
            Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel]
        )
    ]
    public async Task MeetNatsume(User user)
    {
        var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));

        await RespondAsync(
            callback: InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral | MessageFlags.Loading),
            cancellationToken: cts.Token
        );

        var contact = await natsumeContactService.GetNatsumeContactByIdAsync(
            discordId: user.Id,
            cancellationToken: cts.Token
        );

        if (contact is not null)
        {
            await ModifyResponseAsync(
                action: m => m.WithContent($"Natsume-san conosce già {contact.DiscordNickname}!"),
                cancellationToken: cts.Token
            );

            return;
        }

        var dmChannel = await user.GetDMChannelAsync(cancellationToken: cts.Token);

        var newContact = await natsumeContactService.AddNatsumeContactAsync(
            discordId: user.Id,
            discordNickname: user.GetName(),
            cancellationToken: cts.Token
        );

        var welcomePrompt =
            $"""
             Scrivi un breve messaggio in chat in cui ti presenti a {newContact.DiscordNickname},
             usando anche espressioni tipiche giapponesi.
             Aggiungi che quando ha bisogno può sempre scriverti e che speri andrete d'accordo e che
             {newContact.DiscordNickname} sia gentile con te.
             Approfittane per chiedere a {newContact.DiscordNickname} se ha mai studiato una tecnologia di programmazione 
             a tua scelta!
             """;

        var welcome = await _natsumeAi.GetChatCompletionTextAsync(
            NatsumeChatModel.Gpt4O,
            ContactNickname,
            welcomePrompt
        );

        await dmChannel.SendMessageAsync(
            message: new MessageProperties().WithContent(content: welcome),
            cancellationToken: cts.Token
        );

        await ModifyResponseAsync(
            action: m => m.WithContent($"Natsume-san e {newContact.DiscordNickname} hanno stretto amicizia!"),
            cancellationToken: cts.Token
        );
    }

    [
        UserCommand(
            name: "Fai amicizia con Natsume-san!",
            DefaultGuildUserPermissions = Permissions.Administrator,
            Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel]
        )
    ]
    public async Task BefriendNatsume(User user)
    {
        var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));

        await RespondAsync(
            callback: InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral | MessageFlags.Loading),
            cancellationToken: cts.Token
        );

        var contact = await natsumeContactService.GetNatsumeContactByIdAsync(
            discordId: user.Id,
            cancellationToken: cts.Token
        );

        if (contact is null)
        {
            await ModifyResponseAsync(
                action: m =>
                    m.WithContent($"Natsume-san non conosce affatto {user.GetName()}!"),
                cancellationToken: cts.Token
            );

            return;
        }

        if (contact is { IsFriend: true })
        {
            await ModifyResponseAsync(
                action: m =>
                    m.WithContent($"Natsume-san è già amica di {contact.DiscordNickname}!"),
                cancellationToken: cts.Token
            );

            return;
        }

        var dmChannel = await user.GetDMChannelAsync(cancellationToken: cts.Token);

        await natsumeContactService.UpdateNatsumeContactsAsync(
            contacts: contact.Befriend(),
            cancellationToken: cts.Token
        );

        var welcomeBackPrompt =
            $"""
             Scrivi un breve messaggio in chat di bentornato a {contact.DiscordNickname}!
             Usa anche espressioni tipicamente giapponesi!
             Ricorda a {contact.DiscordNickname} che quando ha bisogno può sempre scriverti!
             Approfittane per suggerire a {contact.DiscordNickname} di studiare una tecnologia di 
             programmazione a tua scelta!
             """;

        var welcomeBack = await _natsumeAi.GetChatCompletionTextAsync(
            NatsumeChatModel.Gpt4O,
            ContactNickname,
            welcomeBackPrompt
        );

        await dmChannel.SendMessageAsync(
            message: new MessageProperties().WithContent(welcomeBack),
            cancellationToken: cts.Token
        );

        await ModifyResponseAsync(
            action: m => m
                .WithContent($"Natsume-san ha riabbracciato {contact.DiscordNickname}!"),
            cancellationToken: cts.Token
        );
    }

    [UserCommand(name: "Litiga con Natsume-san",
        DefaultGuildUserPermissions = Permissions.Administrator,
        Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel])]
    public async Task UnfriendNatsume(User user)
    {
        var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));

        await RespondAsync(
            callback: InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral | MessageFlags.Loading),
            cancellationToken: cts.Token
        );

        var contact = await natsumeContactService.GetNatsumeContactByIdAsync(
            discordId: user.Id,
            cancellationToken: cts.Token
        );

        if (contact is null)
        {
            await ModifyResponseAsync(
                action: m =>
                    m.WithContent($"Natsume-san non conosce affatto {user.GetName()}!"),
                cancellationToken: cts.Token
            );

            return;
        }

        if (contact is { IsFriend: false })
        {
            await ModifyResponseAsync(
                action: m =>
                    m.WithContent($"Natsume-san è già arrabbiata con {contact.DiscordNickname}!"),
                cancellationToken: cts.Token
            );

            return;
        }

        var dmChannel = await user.GetDMChannelAsync(cancellationToken: cts.Token);

        await natsumeContactService.UpdateNatsumeContactsAsync(
            contacts: contact.Unfriend(),
            cancellationToken: cts.Token
        );

        var goodbyePrompt =
            $"""
             Scrivi un breve messaggio in chat di addio a {contact.DiscordNickname}!
             Usa anche espressioni tipicamente giapponesi!
             Ringrazia {contact.DiscordNickname} per tutti i messaggi che vi siete scritti.
             Augura a {contact.DiscordNickname} buona fortuna e dì che speri vi incontrerete ancora
             """;

        var goodbye = await _natsumeAi.GetChatCompletionTextAsync(
            NatsumeChatModel.Gpt4O,
            ContactNickname,
            goodbyePrompt
        );

        await dmChannel.SendMessageAsync(
            message: new MessageProperties().WithContent(goodbye),
            cancellationToken: cts.Token
        );

        await ModifyResponseAsync(
            action: m => m
                .WithContent($"Natsume-san ora tiene il broncio con {contact.DiscordNickname}"),
            cancellationToken: cts.Token
        );
    }
}