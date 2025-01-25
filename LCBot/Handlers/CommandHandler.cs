using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LCBot.Modules;

namespace LCBot.Handlers;

public class CommandHandler : DiscordClientService
{
    private readonly IServiceProvider _provider;
    private readonly CommandService _commandService;
    private readonly IConfiguration _config;
    private readonly LeagueModule _leagueModule;

    public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config, LeagueModule leagueModule) : base(client, logger)
    {
        _provider = provider;
        _commandService = commandService;
        _config = config;
        _leagueModule = leagueModule;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Attach event handlers for client events
        Client.MessageReceived += HandleMessage;
        Client.ButtonExecuted += HandleButton; // Add this line for handling button interactions

        // Attach event handlers for command execution
        _commandService.CommandExecuted += CommandExecutedAsync;

        // Load command modules
        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
    }

    private async Task HandleMessage(SocketMessage incomingMessage)
    {
        if (incomingMessage is not SocketUserMessage message) return;
        if (message.Source != MessageSource.User) return;

        int argPos = 0;
        if (!message.HasStringPrefix(_config["Prefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)) return;

        var context = new SocketCommandContext(Client, message);
        await _commandService.ExecuteAsync(context, argPos, _provider);
    }

    private async Task HandleButton(SocketMessageComponent component)
    {
        var customId = component.Data.CustomId;
        if (customId.StartsWith("pick_")) await _leagueModule.PickButtonAsync(component);
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        //Logger.LogInformation("User {user} attempted to use command {command}", context.User, command.Value.Name);

        if (!command.IsSpecified || result.IsSuccess)
            return;

        await context.Channel.SendMessageAsync($"Error: {result}");
    }
}