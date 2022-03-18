using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Commands.SlashCommands;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents;

public static class SlashCommandHandler
{
    public static Task OnCommand(SocketSlashCommand command)
    {
        foreach (var cmd in ApplicationCommandHandler.AllSlashCommands.Where(cmd => cmd.Slashie.Name.Value == command.Data.Name))
        {
            if (cmd.DevOnly && !Constants.Users.IsDev(command.User))
            {
                command.RespondAsync("This is a developer only command.", ephemeral: true);
                break;
            }

            cmd.OnExecute(command);
        }

        return Task.CompletedTask;
    }
}