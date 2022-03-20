using Discord;
using Discord.WebSocket;

namespace TLCBot2.ApplicationComponents.Core;

public class FireMessageCommand
{
    public MessageCommandProperties Command;
    public MessageCommandBuilder Builder;
    public Action<SocketMessageCommand>? OnExecuted;
    public bool DevOnly;
    public SocketGuild? Guild = null;
    public FireMessageCommand(
        MessageCommandBuilder MessageCommand,
        Action<SocketMessageCommand> onExecuted,
        bool devOnly = false)
    {
        Builder = MessageCommand;
        Command = MessageCommand.Build();
        OnExecuted = onExecuted;
        DevOnly = devOnly;
    }
    public static async Task CreateNew(FireMessageCommand command, SocketGuild? guild)
    {
        await command.Create(guild);
    }
    public Task Create(SocketGuild? guild)
    {
        Guild = guild;
        ApplicationCommandManager.AllMessageCommands.Add(this);
        return Task.CompletedTask;
    }
}