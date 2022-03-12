using Discord.WebSocket;

namespace TLCBot2.ApplicationComponents;

public static class ContextCommandHandler
{
    public static void Initialize()
    {
        
    }

    public static Task OnMessageCommandExecuted(SocketMessageCommand commmand)
    {

        return Task.CompletedTask;
    }

    public static Task OnUserCommandExecuted(SocketUserCommand command)
    {

        return Task.CompletedTask;
    }
}