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
        RuntimeConfig.CommandExecutionLog.SendMessageAsync(embed: new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("Slash Command Executed")
            .AddField("Command Name", command.CommandName)
            .AddField("Parameters Used", 
                command.Data.Options is not null && command.Data.Options.Count > 0 
                    ? string.Join("\n", command.Data.Options.Select(x => $"{x.Name}: {x.Value}"))
                    : "None")
            .WithDescription($"User: {command.User.Mention} : {command.User.Id}")
            .WithAuthor(command.User).Build());

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