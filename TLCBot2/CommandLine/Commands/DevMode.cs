using TLCBot2.Attributes;
using TLCBot2.Core;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Enters Developer Mode in the bot. Only use if you know what you're doing.")]
    public static async Task DevMode(bool enabled)
    {
        Program.DeveloperMode = enabled;
        await ChannelTerminal.PrintAsync($"Developer mode has been {(enabled ? "" : "de")}activated");
    }
}