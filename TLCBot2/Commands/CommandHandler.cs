using Discord.WebSocket;
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
}