using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
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
                .WithButton(new ButtonBuilder()
                    .WithLabel("Documentation")
                    .WithUrl("https://github.com/Firebreak-2/TLCBot2/wiki/How-To-Use-Log-Queries")
                    .WithStyle(ButtonStyle.Link)
                )
            ),
            (StringPrompts.ModboardManageEventActions, new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Current Event Actions")
                    .WithCustomId("ea-button;ls,0")
                    .WithStyle(ButtonStyle.Primary)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Add Event Action")
                    .WithCustomId("ea-button;add,0")
                    .WithStyle(ButtonStyle.Primary)
                )
                .WithButton(new ButtonBuilder()
                    .WithLabel("Documentation")
                    .WithUrl("https://example.com")
                    .WithStyle(ButtonStyle.Link)
                )
            ),
            (StringPrompts.ModboardModProfessions, new ComponentBuilder()
                .WithSelectMenu(new Func<SelectMenuBuilder>(() =>
                    {
                        var builder = new SelectMenuBuilder()
                            .WithPlaceholder("Select Your Professions")
                            .WithCustomId("mod-profession-rolemenu");
                        
                        var roles = RuntimeConfig.ModProfessionRoles
                            .Select(x => (x.GetRole(), x.Description));

                        builder.WithOptions(roles.Select(x =>
                            {
                                var optionBuilder = new SelectMenuOptionBuilder()
                                    .WithLabel(x.Item1.Name)
                                    .WithValue(x.Item1.Id.ToString())
                                    .WithDescription(x.Description);

                                if (RuntimeConfig.BackendServer is { } guild
                                    && guild.Emotes.TryFirst(e => e.Name == $"e_{x.Item1.Color.ToString()[1..]}",
                                        out var emote))
                                    optionBuilder.WithEmote(emote);
                                
                                return optionBuilder;
                            })
                            .ToList());

                        return builder
                            .WithMinValues(0)
                            .WithMaxValues(roles.Count());
                    })()
                )
            )
        );
    }
}