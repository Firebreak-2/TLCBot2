using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Commands;

public static class CommandHandler
{
    public static List<FireCommand> AllCommands = new();
    public static Task OnCommand(SocketSlashCommand command)
    {
        foreach (var cmd in AllCommands.Where(cmd => cmd.Slashie.Name.Value == command.Data.Name))
        {
            if (cmd.DevOnly && !Constants.Users.IsDev(command.User))
            {
                command.RespondAsync("This is a developer only command.", ephemeral: true);
                return Task.CompletedTask;
            }

            cmd.OnExecute(command);
        }

        return Task.CompletedTask;
    }

    public static async Task Initialize()
    {
        await TestCommands.Initialize();
        await CommercialCommands.Initialize();
        
        await Constants.Guilds.Lares!.BulkOverwriteApplicationCommandAsync(AllCommands
            .Where(x => x.Guild != null).Select(x => x.Slashie).ToArray());
        
        if (AllCommands.Any(x => x.Guild == null))
            await Program.Client.BulkOverwriteGlobalApplicationCommandsAsync(AllCommands
                .Where(x => x.Guild == null).Select(x => x.Slashie).ToArray());
    }
}