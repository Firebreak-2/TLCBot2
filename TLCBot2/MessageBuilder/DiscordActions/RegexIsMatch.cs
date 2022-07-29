using System.Text.RegularExpressions;
using JetBrains.Annotations;
using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static bool RegexIsMatch([RegexPattern] string pattern, string input)
    {
        return Regex.IsMatch(input, pattern);
    }
}