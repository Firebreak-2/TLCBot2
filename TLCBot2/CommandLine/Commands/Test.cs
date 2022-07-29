using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "A test command that will likely do something unexpected. Leave this for Firebreak.")]
    public static async Task Test()
    {
        foreach (var emote in RuntimeConfig.FocusServer!.Emotes)
        {
            await RuntimeConfig.FocusServer.DeleteEmoteAsync(emote);
        }
    }
}