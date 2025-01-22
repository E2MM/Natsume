using Natsume.LiteDB;
using Natsume.NetCord.NatsumeAI;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

public class NatsumeHqUserCommandModule(LiteDbService liteDbService, NatsumeAi natsumeAi)
    : NatsumeAiCommandModule(natsumeAi)
{
    private readonly NatsumeAi _natsumeAi = natsumeAi;

    [UserCommand(name: "Presentami a Natsume-san!",
        DefaultGuildUserPermissions = Permissions.Administrator,
        Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel])]
    public async Task BefriendNatsume(User user)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral | MessageFlags.Loading));

        var contact = liteDbService.GetNatsumeContactById(user.Id);
        if (contact?.IsNatsumeFriend is true)
        {
            await ModifyResponseAsync(m => m
                .WithContent($"Natsume-san è già amica di {contact.Nickname}!"));
            return;
        }

        var dmChannel = await user.GetDMChannelAsync();

        if (contact?.IsNatsumeFriend is false)
        {
            contact = contact.Befriend();
            liteDbService.UpdateNatsumeContact(contact);

            var welcomeBackPrompt =
                $"""
                 Scrivi un breve messaggio in chat di bentornato a {contact.Nickname}!
                 Usa anche espressioni tipicamente giapponesi!
                 Ricorda a {contact.Nickname} che quando ha bisogno può sempre scriverti!
                 Approfittane per suggerire a {contact.Nickname} di studiare una tecnologia di 
                 programmazione a tua scelta!
                 """;

            var welcomeBack = await _natsumeAi.GetChatCompletionTextAsync(
                NatsumeChatModel.Gpt4O,
                ContactNickname,
                welcomeBackPrompt
            );

            await dmChannel.SendMessageAsync(new MessageProperties()
                .WithContent(welcomeBack));

            await ModifyResponseAsync(m => m
                .WithContent($"Natsume-san ha riabbracciato {contact.Nickname}!"));
            return;
        }

        var newContact = new NatsumeContact(user.Id, user.GetName());
        liteDbService.AddNatsumeContact(newContact);

        var welcomePrompt =
            $"""
             Scrivi un breve messaggio in chat in cui ti presenti a {newContact.Nickname},
             usando anche espressioni tipiche giapponesi.
             Aggiungi che quando ha bisogno può sempre scriverti e che speri andrete d'accordo e che
             {newContact.Nickname} sia gentile con te.
             Approfittane per chiedere a {newContact.Nickname} se ha mai studiato una tecnologia di programmazione 
             a tua scelta!
             """;

        var welcome = await _natsumeAi.GetChatCompletionTextAsync(
            NatsumeChatModel.Gpt4O,
            ContactNickname,
            welcomePrompt
        );

        await dmChannel.SendMessageAsync(new MessageProperties()
            .WithContent(welcome));

        await ModifyResponseAsync(m => m
            .WithContent($"Natsume-san e {newContact.Nickname} hanno stretto amicizia!"));
    }

    [UserCommand(name: "Dì addio a Natsume-san",
        DefaultGuildUserPermissions = Permissions.Administrator,
        Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel])]
    public async Task UnfriendNatsume(User user)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral | MessageFlags.Loading));

        var contact = liteDbService.GetNatsumeContactById(user.Id);
        if (contact is null)
        {
            await ModifyResponseAsync(m => m
                .WithContent($"Natsume-san non conosce affatto {user.GlobalName ?? user.Username}!"));
            return;
        }

        contact = contact.Unfriend();
        liteDbService.UpdateNatsumeContact(contact);

        var goodbyePrompt =
            $"""
             Scrivi un breve messaggio in chat di addio a {contact.Nickname}!
             Usa anche espressioni tipicamente giapponesi!
             Ringrazia {contact.Nickname} per tutti i messaggi che vi siete scritti.
             Augura a {contact.Nickname} buona fortuna e dì che speri vi incontrerete ancora
             """;

        var goodbye = await _natsumeAi.GetChatCompletionTextAsync(
            NatsumeChatModel.Gpt4O,
            ContactNickname,
            goodbyePrompt
        );

        var dmChannel = await user.GetDMChannelAsync();
        await dmChannel.SendMessageAsync(new MessageProperties()
            .WithContent(goodbye));

        await ModifyResponseAsync(m => m
            .WithContent($"Natsume-san ha detto addio a {contact.Nickname}"));
    }

    [UserCommand(name: "Fai un regalino a Natsume-san!",
        DefaultGuildUserPermissions = Permissions.Administrator,
        Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel])]
    public async Task TipNatsume(User user)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral | MessageFlags.Loading));

        var contact = liteDbService.GetNatsumeContactById(user.Id);
        if (contact is null)
        {
            await ModifyResponseAsync(m => m
                .WithContent(
                    $"Natsume-san non conosce {user.GetName()}! Non può accettare regalini da sconosciuti"));
            return;
        }

        contact.AwardFriendship(0.1M);
        liteDbService.UpdateNatsumeContact(contact);

        var thankYouPrompt =
            $"""
             Scrivi un breve messaggio in chat a {contact.Nickname} per ringraziare del regalino!
             E' super kawaiii!
             """;

        var thankYou = await _natsumeAi.GetChatCompletionTextAsync(
            NatsumeChatModel.Gpt4O,
            ContactNickname,
            thankYouPrompt
        );

        var dmChannel = await user.GetDMChannelAsync();
        await dmChannel.SendMessageAsync(new MessageProperties()
            .WithContent(thankYou));

        await ModifyResponseAsync(m => m
            .WithContent($"Natsume-san è così felice del regalino ricevuto da {contact.Nickname}!"));
    }
}