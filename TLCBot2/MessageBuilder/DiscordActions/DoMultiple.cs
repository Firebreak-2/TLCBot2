using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void DoMultiple(MessageComponentAction[] actions)
    {
        foreach (var action in actions)
        {
            action.Execute();
        }
    }
}