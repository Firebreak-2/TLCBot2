using TLCBot2.Attributes;
using TLCBot2.Data.RuntimeConfig;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Manually saves all the current runtime configuration fields.")]
    public static async Task SaveConfig()
    {
        await Task.Run(RuntimeConfig.SaveConfig);
        await ChannelTerminal.PrintAsync("Saved current config values");
    }
}