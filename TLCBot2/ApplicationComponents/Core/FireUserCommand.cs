using Discord;
using Discord.WebSocket;

namespace TLCBot2.ApplicationComponents.Core;

public class FireUserCommand
{
    public UserCommandProperties Command;
    public Action<SocketUserCommand>? OnExecuted;
    public bool DevOnly;
    public SocketGuild? Guild = null;
    public FireUserCommand(
        UserCommandBuilder userCommand,
        Action<SocketUserCommand> onExecuted,
        bool devOnly = false)
    {
        Command = userCommand.Build();
        OnExecuted = onExecuted;
        DevOnly = devOnly;
    }
    public static async Task CreateNew(FireUserCommand command, SocketGuild? guild)
    {
        await command.Create(guild);
    }
    public Task Create(SocketGuild? guild)
    {
        Guild = guild;
        ApplicationCommandHandler.AllUserCommands.Add(this);
        return Task.CompletedTask;
    }
}