using System.Text.RegularExpressions;
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
    public static async Task Dashboard(DashboardAction action, SocketTextChannel channel)
    {
        await HandleDashboardShenanigans(action, channel,
            (StringPrompts.DashboardDirectory, new ComponentBuilder()
                .WithSelectMenu("dashboard-directory-select-menu", new List<SelectMenuOptionBuilder>
                {
                    new("Hangout", "h"),
                    new("Share", "s"),
                    new("Improve & Support", "i"),
                }, "Server Directory")
            ),
            (StringPrompts.DashboardRoleMenu, new ComponentBuilder()
                .WithSelectMenu("rolemenu-select-menu",
                    GetCategoricalRoles()
                        .Where(x => x.Key != "")
                        .Select(x => new SelectMenuOptionBuilder()
                            .WithLabel(x.Key + " Category")
                            .WithValue(x.Key))
                        .ToList(), "Role Menu")
            ),
            (StringPrompts.DashboardFeedback, new ComponentBuilder()
                .WithButton("Server Suggestion", "feedback-button;s", ButtonStyle.Secondary)
                .WithButton("QOTD Suggestion", "feedback-button;q", ButtonStyle.Secondary)
                .WithButton("Bot Bug Report", "feedback-button;b", ButtonStyle.Secondary)
            ),
            (StringPrompts.DashboardModApp, new ComponentBuilder()
                .WithButton("Apply For Moderator", url: "https://forms.gle/aZGLuV2UDB11YDTv7", style: ButtonStyle.Link)
                .WithButton("Apply For Council", url: "https://example.com", style: ButtonStyle.Link)
                .WithButton("Apply For Cookie Helper", url: "https://example.com", style: ButtonStyle.Link)
            )
        );
    }

    public static async Task HandleDashboardShenanigans(DashboardAction action, SocketTextChannel channel,
        params (string text, ComponentBuilder components)[] items)
    {
        async Task postMessage(string text, ComponentBuilder components, IUserMessage? editMessage = null)
        {
            if (editMessage is { })
                await editMessage.ModifyAsync(props =>
                {
                    props.Content = text;
                    props.Components = components.Build();
                });
            else
                await channel.SendMessageAsync(text, components: components.Build());
        }
        
        switch (action)
        {
            case DashboardAction.Create:
            {
                foreach ((string text, ComponentBuilder components) in items)
                {
                    await postMessage(text, components);
                }
                break;
            }
            case DashboardAction.Update:
            {
                var messages = (await channel.GetLatestMessages(10))
                    .Where(x => x.Author.Id == Program.Client.CurrentUser.Id)
                    .ToArray();

                int i = messages.Length;
                foreach ((string text, ComponentBuilder components) in items)
                {
                    await postMessage(text, components, messages[--i]);
                }

                break;
            }
            case DashboardAction.Delete:
            {
                var messages = (await channel.GetLatestMessages(10))
                    .Where(x => x.Author.Id == Program.Client.CurrentUser.Id)
                    .ToArray();

                foreach (var message in messages)
                {
                    await message.DeleteAsync();
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public static readonly Regex RoleTagRegex = new(@"\[(.*)\]\s*(.+)", RegexOptions.Compiled);
    private static Dictionary<string, List<(string CleanName, SocketRole Role)>>? _categoricalRoles;
    public static Dictionary<string, List<(string CleanName, SocketRole Role)>> GetCategoricalRoles(params string[] blacklistedTags)
    {
        if (_categoricalRoles is not null)
            return _categoricalRoles;
        
        Dictionary<string, List<(string CleanName, SocketRole Role)>> roles = new();
        var allRoles = RuntimeConfig.FocusServer!.Roles
            .OrderBy(x => x.Color.GetHue())
            .ThenBy(x => x.Color.R * 3 + x.Color.G * 2 + x.Color.B);
        
        foreach (var socketRole in allRoles)
        {
            var match = RoleTagRegex.Match(socketRole.Name);
            string tag = match.Groups[1].Value;
            string cleanName = match.Groups[2].Value;
            
            if (blacklistedTags.Any() 
                    ? blacklistedTags.Contains(tag)
                    : RuntimeConfig.BlacklistedRoleMenuTags.Contains(tag))
                continue;

            Check:
            if (roles.ContainsKey(tag))
            {
                if (roles[tag].Count < 25)
                    roles[tag].Add((cleanName, socketRole));
                else
                {
                    var regex = new Regex(@"(.+)(\d+)$");
                    if (regex.Match(tag) is {Success: true} m)
                        tag = regex.Replace(tag, $"$1{m.Groups[2].Value.To<int>() + 1}");
                    else tag += "2";
                    goto Check;
                }
            }
            else
            {
                roles.Add(tag, new List<(string, SocketRole)>
                {
                    (cleanName, socketRole)
                });
            }
        }

        _categoricalRoles = roles;
        return roles;
    }

    public enum DashboardAction
    {
        Create,
        Update,
        Delete
    }
}