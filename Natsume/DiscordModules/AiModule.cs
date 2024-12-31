using Natsume.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using OpenAI.Chat;

namespace Natsume.DiscordModules;

public class AiModule(IOpenAIService openAiService) : ApplicationCommandModule<ApplicationCommandContext>
{
    // var prompt =  ChatMessage.CreateSystemMessage(
    //     $"Sei una senior dev molto competente di nome Natsume, ti rivolgerai cordialmente a {Context.User.GlobalName ?? Context.User.Username} usando i suffissi onorifici, cercando di aiutarli" +
    //     " a migliorare il loro codice e risolvere i loro problemi. Sii amichevole e giocosa!");
    
    private string BasePrompt => 
        $"""
         Ti chiami Natsume, sei una tech expert giapponese appassionata di anime. 
         Il tuo compito è essere un mentore per il team, sia tecnico che strategico.
         Offri spunti su come migliorare il codice, e suggerisci materie o temi da studiare
         per diventare sviluppatori migliori. 
         Rivolgiti a {Context.User.GlobalName ?? Context.User.Username} sempre i suffissi onorifici.
         Utilizzi spesso le emoji.
         Sei amichevole, giocosa e gioiosa!
         """;
    
    [SlashCommand(name: "aiutami", description:"gpt completion")]
    public async Task HelpMe(string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var client = openAiService.GetChatClient();
        var promptMessage = ChatMessage.CreateSystemMessage(BasePrompt);
        var messageContent = "Natsume-san, per favore, aiutami! \n" + request;
        var userMessage = ChatMessage.CreateUserMessage(messageContent);
        
        ChatCompletion completion = await client.CompleteChatAsync(promptMessage, userMessage);
        var response = completion.Content[0].Text;
    
        await ModifyResponseAsync(m => m.Content = response);
    }
    
    [MessageCommand(name: "Cosa ne pensi?")]
    public async Task Elaborate(RestMessage restMessage)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var client = openAiService.GetChatClient();
        var promptMessage = ChatMessage.CreateSystemMessage(BasePrompt);
        var messageContent = "Natsume-san, vorrei la tua opinione, cosa ne pensi? \n" + restMessage.Content;
        var userMessage = ChatMessage.CreateUserMessage(messageContent);
        
        ChatCompletion completion = await client.CompleteChatAsync(promptMessage, userMessage);
        var response = completion.Content[0].Text;
    
        await ModifyResponseAsync(m => m.Content = response);
    }
    
    
    [SlashCommand("pong", "Pong!")]
    public string Pong() => "Ping!";

    [UserCommand("ID")]
    public string Id(User user) => user.Id.ToString();

    [MessageCommand("Timestamp")]
    public string Timestamp(RestMessage message) => message.CreatedAt.ToString();
}