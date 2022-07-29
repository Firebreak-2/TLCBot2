using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void DoMultiple(SocketMessageComponent? executor, MessageComponentAction[] actions)
    {
        foreach (var action in actions)
        {
            action.Execute<object>(executor);
        }
    }
}