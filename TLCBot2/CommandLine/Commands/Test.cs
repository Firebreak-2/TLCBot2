using TLCBot2.Attributes;
using TLCBot2.Data;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "A test command that will likely do something unexpected. Leave this for Firebreak.")]
    public static async Task Test()
    {
        await using var db = new TlcDbContext();
        var results = db.Logs
            .OrderByDescending(x => x.ID)
            .Take(10)
            .Select(x => $"{x.ID}\n{x.Message}");
        await ChannelTerminal.PrintAsync(string.Join("```\n```\n", results));
    }
}