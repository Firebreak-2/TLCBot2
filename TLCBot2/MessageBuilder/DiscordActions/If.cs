using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void If(
        object? executor,
        bool condition,
        MessageComponentAction then,
        MessageComponentAction? @else)
    {
        if (condition)
            then.Execute<object>(executor);
        else
            @else?.Execute<object>(executor);
    }
}