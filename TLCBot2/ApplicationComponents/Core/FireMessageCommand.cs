using Discord;
using Discord.WebSocket;

namespace TLCBot2.ApplicationComponents.Core;

public class FireMessageCommand
{
    public MessageCommandProperties Command;
    public Action<SocketMessageCommand>? OnExecuted;
    public bool DevOnly;
    public SocketGuild? Guild = null;
    public FireMessageCommand(
        MessageCommandBuilder MessageCommand,
        Action<SocketMessageCommand> onExecuted)
    {
        Command = MessageCommand.Build();
        OnExecuted = onExecuted;
    }
    public static async Task CreateNew(FireMessageCommand command, SocketGuild? guild)
    {
        await command.Create(guild);
    }
    public Task Create(SocketGuild? guild)
    {
        Guild = guild;
        ApplicationCommandHandler.AllMessageCommands.Add(this);
        return Task.CompletedTask;
    }
}