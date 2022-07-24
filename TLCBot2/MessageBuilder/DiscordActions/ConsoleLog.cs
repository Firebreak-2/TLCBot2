using TLCBot2.Attributes;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void ConsoleLog(string message) => Console.WriteLine(message);
}