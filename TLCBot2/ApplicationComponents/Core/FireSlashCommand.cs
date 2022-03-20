using Discord;
using Discord.WebSocket;

namespace TLCBot2.ApplicationComponents.Core;

public class FireSlashCommand
{
    public SlashCommandProperties Slashie;
    public readonly SlashCommandBuilder Builder;
    public Action<SocketSlashCommand> OnExecute;
    public bool DevOnly;
    public SocketGuild? Guild = null;
    public FireSlashCommand(SlashCommandBuilder slashie, Action<SocketSlashCommand> onExecute, bool devOnly = false)
    {
        Builder = slashie;
        Slashie = slashie.Build();
        OnExecute = onExecute;
        DevOnly = devOnly;
    }

    public static async Task CreateNew(FireSlashCommand slashCommand, SocketGuild? guild)
    {
        await slashCommand.Create(guild);
    }
    public Task Create(SocketGuild? guild)
    {
        Guild = guild;
        ApplicationCommandManager.AllSlashCommands.Add(this);
        return Task.CompletedTask;
    }
}