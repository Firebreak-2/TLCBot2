using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void Pipe(object? executor, MessageComponentAction from, MessageComponentAction to)
    {
        to.Execute<object>(from.Execute<object>(executor));
    }
}