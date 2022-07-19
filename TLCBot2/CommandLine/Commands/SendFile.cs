using TLCBot2.Attributes;
using TLCBot2.Core;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Sends a file from the TLC_Files directory")]
    public static async Task SendFile(string fileName)
    {
        await ChannelTerminal.Channel.SendFileAsync($"{Program.FileAssetsPath}/{fileName}", fileName);
    }
}