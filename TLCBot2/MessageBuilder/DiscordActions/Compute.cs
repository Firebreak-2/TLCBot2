using System.Data;
using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    private static readonly DataTable s_table = new();
    
    [DiscordAction]
    public static object Compute(string from)
    {
        return s_table.Compute(from, null);
    }
}