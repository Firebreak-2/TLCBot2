using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data.StringPrompts;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand]
    public static async Task Botboard(DashboardAction action, SocketTextChannel channel)
    {
        await HandleDashboardShenanigans(action, channel);
    }
}