using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public new static bool Equals(object item1, object item2)
    {
        return item1 == item2;
    }
}