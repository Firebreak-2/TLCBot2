using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void Foreach(object? executor, object[] items, MessageComponentAction action)
    {
        int i = 0;
        foreach (object item in items)
        {
            action.Execute<object>(executor, ("$%item%", item), ("$%i%", i++));
        }
    }
}