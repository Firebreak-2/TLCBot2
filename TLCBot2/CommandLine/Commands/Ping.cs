using Discord;
using TLCBot2.Attributes;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Confirms that the bot is alive and can respond.")]
    public static async Task Ping()
    {
        await ChannelTerminal.PrintAsync($"{Helper.Ansi.Foreground.Green.Get()}Pong!");
    }
}