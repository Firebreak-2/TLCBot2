using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.CommandLine.Commands;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [ComponentInteraction("rolemenu-select-menu")]
    public async Task RoleMenuSelectMenuResponse(string[] selectedOptions)
    {
        var allRoles = TerminalCommands.GetCategoricalRoles();
        string selected = selectedOptions[0];
        
        if (!allRoles.ContainsKey(selected))
            return;

        var selectedRoles = allRoles[selected];

        await RespondAsync(
            string.Join('\n', selectedRoles.Select(x => x.Role.Mention)),
            components: new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                    .WithCustomId($"rolemenu-category-select-menu;true,{selected}")
                    .WithPlaceholder($"{selected} Roles")
                    .WithOptions(selectedRoles
                        .Select(x =>
                        {
                            var optionBuilder = new SelectMenuOptionBuilder(x.CleanName, $"{x.Role.Id}");

                            if (RuntimeConfig.BackendServer is { } guild
                                && guild.Emotes.TryFirst(e => e.Name == $"e_{x.Role.Color.ToString()[1..]}",
                                    out var emote))
                                optionBuilder.WithEmote(emote);
                            
                            return optionBuilder;
                        })
                        .ToList()))
                .WithButton("Clear", $"rolemenu-category-clear-button;{selected}")
                .Build(),
            ephemeral: true);
    }

    [ComponentInteraction("rolemenu-category-select-menu;*,*")]
    public async Task RoleMenuCategoryResponse(string oneRoleString, string category, string[] selectedRoles)
    {
        bool oneRole = bool.Parse(oneRoleString);
        SocketGuildUser user = (SocketGuildUser) Context.User;
        var roles = TerminalCommands.GetCategoricalRoles()[category];
        var userRoles = user.Roles;
        
        var selectedRole = roles.First(x => x.Role.Id.ToString() == selectedRoles[0]).Role;
        if (!userRoles.Contains(selectedRole))
            await user.AddRoleAsync(selectedRole);

        if (oneRole)
        {
            await user.RemoveRolesAsync(roles
                .Select(x => x.Role)
                .Where(x => userRoles.Contains(x)));
        }
        
        await DeferAsync();
    }

    [ComponentInteraction("rolemenu-category-clear-button;*")]
    public async Task RoleMenuCategoryResponse(string category)
    {
        SocketGuildUser user = (SocketGuildUser) Context.User;
        var roles = TerminalCommands.GetCategoricalRoles()[category];
        var userRoles = user.Roles;

        await user.RemoveRolesAsync(roles
            .Select(x => x.Role)
            .Where(x => userRoles.Contains(x)));

        await DeferAsync();
    }
}