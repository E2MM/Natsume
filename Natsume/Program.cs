using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Natsume.DiscordModules;
using Natsume.Services;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddUserSecrets<Program>();

var discordToken = builder.Configuration.GetSection("Discord")["Token"];
if(discordToken is null) throw new ApplicationException("Invalid Discord Token");

var openAIApiKey = builder.Configuration.GetSection("OpenAI")["ApiKey"];
if(openAIApiKey is null) throw new ApplicationException("Invalid OpenAI Api Key");

builder.Services
    .AddDiscordGateway(options => options.Token = discordToken)
    .AddApplicationCommands<ApplicationCommandInteraction, ApplicationCommandContext>()
    .AddSingleton<IOpenAIService, OpenAIService>(f => new OpenAIService(openAIApiKey));

var host = builder.Build();

host.AddApplicationCommandModule<AiModule>();
//host.AddModules(typeof(Program).Assembly);
host.UseGatewayEventHandlers();

await host.RunAsync();