using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.ApplicationComponents.Commands.SlashCommands;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.Core.CommandLine;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents;

public static class SlashCommandHandler
{
    public static Task OnCommand(SocketSlashCommand command)
    {
        foreach (var cmd in ApplicationCommandManager.AllSlashCommands.Where(cmd => cmd.Slashie.Name.Value == command.Data.Name))
        {
            if (cmd.DevOnly && !Constants.Users.IsDev(command.User))
            {
                command.RespondAsync("This is a developer only command.", ephemeral: true);
                break;
            }

            try
            {
                cmd.OnExecute(command);
            }
            catch (Exception e)
            {
                Helper.LogInteractionError($"{JsonConvert.SerializeObject(e, Formatting.Indented)}", "slash command");
                command.RespondAsync(
                    "Uh oh, something failed. The development team has been notified of the error.",
                    ephemeral: true);
            }
        }

        return Task.CompletedTask;
    }
}