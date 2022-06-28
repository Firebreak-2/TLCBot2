using TLCBot2.Attributes;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Repeats the string input in the terminal.")]
    public static async Task Echo(string repeatedString)
    {
        await ChannelTerminal.PrintAsync(repeatedString);
    }
}