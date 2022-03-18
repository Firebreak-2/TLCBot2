using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Commands.MessageCommands;
using TLCBot2.ApplicationComponents.Commands.UserCommands;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents;

public static class ContextCommandHandler
{
    public static Task OnMessageCommandExecuted(SocketMessageCommand command)
    {
        var msgCmds = ApplicationCommandHandler.AllMessageCommands;
        for (int i = 0; i < msgCmds.Count; i++)
        {
            if (msgCmds[i].Command.Name.Value != command.Data.Name) continue;
            
            if (msgCmds[i].DevOnly && !Constants.Users.IsDev(command.User))
            {
                command.RespondAsync("This is a developer only command.", ephemeral: true);
                break;
            }

            msgCmds[i].OnExecuted?.Invoke(command);
        }
        return Task.CompletedTask;
    }

    public static Task OnUserCommandExecuted(SocketUserCommand command)
    {
        var userCmds = ApplicationCommandHandler.AllUserCommands;
        for (int i = 0; i < userCmds.Count; i++)
        {
            if (userCmds[i].Command.Name.Value != command.Data.Name) continue;
            
            if (userCmds[i].DevOnly && !Constants.Users.IsDev(command.User))
            {
                command.RespondAsync("This is a developer only command.", ephemeral: true);
                break;
            }

            userCmds[i].OnExecuted?.Invoke(command);
        }
        return Task.CompletedTask;
    }
}