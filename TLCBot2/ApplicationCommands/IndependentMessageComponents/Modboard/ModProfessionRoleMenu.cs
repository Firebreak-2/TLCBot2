using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [ComponentInteraction("mod-profession-rolemenu")]
    public async Task ManageEventActionsButton(string[] selectedOptions)
    {
        var user = (SocketGuildUser) Context.User;
        var roles = RuntimeConfig.ModProfessionRoles.ToList();
        
        foreach (string selectedOption in selectedOptions)
        {
            if (!roles.TryFirst(x => x.RoleId.ToString() == selectedOption, out var role))
                continue;
            
            if (user.Roles.Any(x => x.Id.ToString() == selectedOption))
            {
                roles.Remove(role);
                continue;
            }

            await user.AddRoleAsync(role.GetRole());
            roles.Remove(role);
        }

        foreach (var role in roles)
        {
            if (user.Roles.All(x => x.Id != role.RoleId))
                continue;

            await user.RemoveRoleAsync(role.GetRole());
        }

        await DeferAsync();
    }
}