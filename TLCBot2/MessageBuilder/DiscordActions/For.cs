using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void For(int cycles, MessageComponentAction action)
    {
        for (int i = 0; i < cycles; i++)
        {
            action.Execute(("i", i));
        }
    }
}