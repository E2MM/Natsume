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
    public async Task AddSubscriber(User user)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral | MessageFlags.Loading));

        var subscriber = liteDbService.GetSubscriberById(user.Id);
        if (subscriber?.ActiveSubscription is true)
        {
            await ModifyResponseAsync(m => m
                .WithContent($"Natsume-san conosce già {subscriber.Username}!"));
            return;
        }

        var dmChannel = await user.GetDMChannelAsync();

        if (subscriber?.ActiveSubscription is false)
        {
            subscriber.ActiveSubscription = true;
            liteDbService.UpdateSubscriber(subscriber);

            var welcomeBackPrompt =
                $"""
                 Scrivi un breve messaggio in chat di bentornato a {subscriber.Username}!
                 Usa anche espressioni tipicamente giapponesi!
                 Ricorda a {subscriber.Username} che quando ha bisogno può sempre scriverti!
                 Approfittane per suggerire a {subscriber.Username} di studiare una tecnologia di 
                 programmazione a tua scelta!
                 """;

            var welcomeBack = await _natsumeAi.GetCompletionTextAsync(
                NatsumeLlmModel.Gpt4O,
                SubscriberName,
                welcomeBackPrompt
            );

            await dmChannel.SendMessageAsync(new MessageProperties()
                .WithContent(welcomeBack));

            await ModifyResponseAsync(m => m
                .WithContent($"Natsume-san ha riabbracciato {subscriber.Username}!"));
            return;
        }

        var newSubscriber = new Subscriber
        {
            Id = user.Id,
            ActiveSubscription = true,
            Username = user.GlobalName ?? user.Username,
            CurrentBalance = 0,
            LastBalanceCharge = null,
            TotalBalanceCharged = 0,
            LastInvocation = null,
            TotalInvocations = 0,
            InputTokensConsumed = 0,
            OutputTokensConsumed = 0
        };

        liteDbService.AddSubscriber(newSubscriber);

        var welcomePrompt =
            $"""
             Scrivi un breve messaggio in chat in cui ti presenti a {newSubscriber.Username},
             usando anche espressioni tipiche giapponesi.
             Aggiungi che quando ha bisogno può sempre scriverti e che speri andrete d'accordo e che
             {newSubscriber.Username} sia gentile con te.
             Approfittane per chiedere a {newSubscriber.Username} se ha mai studiato una tecnologia di programmazione 
             a tua scelta!
             """;

        var welcome = await _natsumeAi.GetCompletionTextAsync(
            NatsumeLlmModel.Gpt4O,
            SubscriberName,
            welcomePrompt
        );

        await dmChannel.SendMessageAsync(new MessageProperties()
            .WithContent(welcome));

        await ModifyResponseAsync(m => m
            .WithContent($"Natsume-san e {newSubscriber.Username} si sono presentati!"));
    }

    [UserCommand(name: "Dì addio a Natsume-san",
        DefaultGuildUserPermissions = Permissions.Administrator,
        Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel])]
    public async Task RemoveSubscriber(User user)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral | MessageFlags.Loading));

        var subscriber = liteDbService.GetSubscriberById(user.Id);
        if (subscriber is null)
        {
            await ModifyResponseAsync(m => m
                .WithContent($"Natsume-san non conosce affatto {user.GlobalName ?? user.Username}!"));
            return;
        }

        subscriber.ActiveSubscription = false;
        liteDbService.UpdateSubscriber(subscriber);

        var goodbyePrompt =
            $"""
             Scrivi un breve messaggio in chat di addio a {subscriber.Username}!
             Usa anche espressioni tipicamente giapponesi!
             Ringrazia {subscriber.Username} per tutti i messaggi che vi siete scritti.
             Augura a {subscriber.Username} buona fortuna e dì che speri vi incontrerete ancora
             """;

        var goodbye = await _natsumeAi.GetCompletionTextAsync(
            NatsumeLlmModel.Gpt4O,
            SubscriberName,
            goodbyePrompt
        );

        var dmChannel = await user.GetDMChannelAsync();
        await dmChannel.SendMessageAsync(new MessageProperties()
            .WithContent(goodbye));

        await ModifyResponseAsync(m => m
            .WithContent($"Natsume-san ha detto addio a {subscriber.Username}"));
    }

    [UserCommand(name: "Fai un regalino a Natsume-san!",
        DefaultGuildUserPermissions = Permissions.Administrator,
        Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel])]
    public async Task RechargeSubscriber(User user)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral | MessageFlags.Loading));

        var subscriber = liteDbService.GetSubscriberById(user.Id);
        if (subscriber is null)
        {
            await ModifyResponseAsync(m => m
                .WithContent(
                    $"Natsume-san non conosce {user.GlobalName ?? user.Username}! Non può accettare regalini da sconosciuti"));
            return;
        }

        subscriber.CurrentBalance += 1M;
        subscriber.TotalBalanceCharged += 1M;
        subscriber.LastBalanceCharge = DateTime.Now;
        liteDbService.UpdateSubscriber(subscriber);

        var thankYouPrompt =
            $"""
             Scrivi un breve messaggio in chat a {subscriber.Username} per ringraziare del regalino!
             E' super kawaiii!
             """;

        var thankYou = await _natsumeAi.GetCompletionTextAsync(
            NatsumeLlmModel.Gpt4O,
            SubscriberName,
            thankYouPrompt
        );

        var dmChannel = await user.GetDMChannelAsync();
        await dmChannel.SendMessageAsync(new MessageProperties()
            .WithContent(thankYou));

        await ModifyResponseAsync(m => m
            .WithContent($"Natsume-san è così felice del regalino ricevuto da {subscriber.Username}!"));
    }
}