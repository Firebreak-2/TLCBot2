using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.ApplicationComponents.Commands.MessageCommands;
using TLCBot2.ApplicationComponents.Commands.UserCommands;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.Core.CommandLine;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents;

public static class ContextCommandHandler
{
    public static Task OnMessageCommandExecuted(SocketMessageCommand command)
    {
        RuntimeConfig.CommandExecutionLog.SendMessageAsync(embed: new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("Message Command Executed")
            .AddField("Command Name", command.CommandName)
            .AddField("Message Used On", command.Data.Message.GetJumpUrl())
            .WithDescription($"User: {command.User.Mention} : {command.User.Id}")
            .WithAuthor(command.User).Build());
        
        var msgCmds = ApplicationCommandManager.AllMessageCommands;
        for (int i = 0; i < msgCmds.Count; i++)
        {
            if (msgCmds[i].Command.Name.Value != command.Data.Name) continue;
            
            if (msgCmds[i].DevOnly && !Constants.Users.IsDev(command.User))
            {
                command.RespondAsync("This is a developer only command.", ephemeral: true);
                break;
            }

            try
            {
                msgCmds[i].OnExecuted?.Invoke(command);
            }
            catch (Exception e)
            {
                Helper.LogInteractionError($"{JsonConvert.SerializeObject(e, Formatting.Indented)}", "message command", command.Data.Message);
                command.RespondAsync(
                    "Uh oh, something failed. The development team has been notified of the error.",
                    ephemeral: true);
            }
            break;
        }
        return Task.CompletedTask;
    }

    public static Task OnUserCommandExecuted(SocketUserCommand command)
    {
        RuntimeConfig.CommandExecutionLog.SendMessageAsync(embed: new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("User Command Executed")
            .AddField("Command Name", command.CommandName)
            .AddField("User Used On", command.Data.Member.Mention)
            .WithDescription($"User: {command.User.Mention} : {command.User.Id}")
            .WithAuthor(command.User).Build());

        var userCmds = ApplicationCommandManager.AllUserCommands;
        for (int i = 0; i < userCmds.Count; i++)
        {
            if (userCmds[i].Command.Name.Value != command.Data.Name) continue;
            
            if (userCmds[i].DevOnly && !Constants.Users.IsDev(command.User))
            {
                command.RespondAsync("This is a developer only command.", ephemeral: true);
                break;
            }

            try
            {
                userCmds[i].OnExecuted?.Invoke(command);
            }
            catch (Exception e)
            {
                Helper.LogInteractionError($"{JsonConvert.SerializeObject(e, Formatting.Indented)}", "user command");
                command.RespondAsync(
                    "Uh oh, something failed. The development team has been notified of the error.",
                    ephemeral: true);
            }
        }
        return Task.CompletedTask;
    }
}