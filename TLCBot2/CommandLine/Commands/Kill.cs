using TLCBot2.Attributes;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Shuts down the bot.")]
    public static Task Kill()
    {
        Helper.Kill();
        return Task.CompletedTask;
    }
}