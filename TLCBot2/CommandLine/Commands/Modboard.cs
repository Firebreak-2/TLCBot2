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
    public static async Task Modboard(DashboardAction action, SocketTextChannel channel)
    {
        await HandleDashboardShenanigans(action, channel,
            (StringPrompts.ModboardLogChannelConfig, new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Change Log Channel")
                    .WithCustomId("logfig-button;cc")
                    .WithStyle(ButtonStyle.Primary)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Change Filter Settings")
                    .WithCustomId("logfig-button;cf")
                    .WithStyle(ButtonStyle.Primary)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Restore Default Filter")
                    .WithCustomId("logfig-button;rf")
                    .WithStyle(ButtonStyle.Danger)
                )
            )
        );
    }
}