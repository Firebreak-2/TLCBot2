using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using MoreLinq;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents.Eternal;

public static class EternalSelectMenus
{
    public static SelectMenuBuilder EternalSelectMenu0(SocketTextChannel channel) => new(
        "ETERNAL-SELECTMENU-0",
        GetColorRoles(channel).Select(x =>
                new SelectMenuOptionBuilder(x.Name.RemovePrefix(), x.Id.ToString())).ToList());
    public static SelectMenuBuilder EternalSelectMenu1(SocketTextChannel channel) => new(
        "ETERNAL-SELECTMENU-1",
        GetPronounRoles(channel).Select(x =>
                new SelectMenuOptionBuilder(x.Name.RemovePrefix(), x.Id.ToString())).ToList());
    public static SelectMenuBuilder EternalSelectMenu2(SocketTextChannel channel) => new(
        "ETERNAL-SELECTMENU-2",
        GetPingRoles(channel).Select(x =>
                new SelectMenuOptionBuilder(x.Name.RemovePrefix(), x.Id.ToString())).ToList());
    public static SelectMenuBuilder EternalSelectMenu3(SocketTextChannel channel) => new(
        "ETERNAL-SELECTMENU-3",
        GetBotFunRoles(channel).Select(x =>
                new SelectMenuOptionBuilder(x.Name.RemovePrefix(), x.Id.ToString())).ToList());
    public static SelectMenuBuilder EternalSelectMenu4(SocketTextChannel channel) => new(
        "ETERNAL-SELECTMENU-4",
        GetArtSpecialityRoles(channel).Select(x =>
                new SelectMenuOptionBuilder(x.Name.RemovePrefix(), x.Id.ToString())).ToList());
    public static SelectMenuBuilder EternalSelectMenu5(SocketTextChannel channel) => new(
        "ETERNAL-SELECTMENU-5",
        GetMiscRoles(channel).Select(x =>
                new SelectMenuOptionBuilder(x.Name.RemovePrefix(), x.Id.ToString())).ToList());
    public static IEnumerable<SocketRole> GetRoles(string prefix, SocketGuild guild) =>
        guild.Roles
            .OrderByDescending(x => x.Position)
            .Where(x => x.Name.StartsWith($"[{prefix}]"));
    public static IEnumerable<SocketRole> GetColorRoles(SocketTextChannel channel) =>
        channel.Guild.Roles
            .OrderByDescending(x => System.Drawing.Color.FromArgb(x.Color.R, x.Color.G, x.Color.B).GetHue())
            .Where(x => x.Name.StartsWith("[Color]"));
    public static IEnumerable<SocketRole> GetPronounRoles(SocketTextChannel channel) =>
        GetRoles("Pronoun", channel.Guild); 
    public static IEnumerable<SocketRole> GetPingRoles(SocketTextChannel channel) =>
        GetRoles("Ping", channel.Guild); 
    public static IEnumerable<SocketRole> GetBotFunRoles(SocketTextChannel channel) =>
        GetRoles("Bot Fun", channel.Guild); 
    public static IEnumerable<SocketRole> GetArtSpecialityRoles(SocketTextChannel channel) =>
        GetRoles("Art Spec", channel.Guild); 
    public static IEnumerable<SocketRole> GetMiscRoles(SocketTextChannel channel) =>
        GetRoles("Misc", channel.Guild);

    private static string RemovePrefix(this string text) => Regex.Replace(text, @"\[.+\] (?=.+)", "");
    public static void OnExecute(SocketMessageComponent selectionMenu)
    {
        const string remove = "ETERNAL-SELECTMENU-";
        int id = int.Parse(selectionMenu.Data.CustomId.Remove(0, remove.Length));

        void RoleReplace(IEnumerable<SocketRole> roles, bool onlyOne = true)
        {
            if (selectionMenu.Data.Values.All(x => x != "rmv"))
            {
                var guild = selectionMenu.Channel.GetGuild();
                var selectedRoles = selectionMenu.Data.Values.Select(x => guild.GetRole(ulong.Parse(x)));
                var user = guild.GetUser(selectionMenu.User.Id);
                if (onlyOne)
                    user.RemoveRolesAsync(roles.Where(user.Roles.Contains));
                MoreEnumerable.ForEach(selectedRoles, x => user.AddRoleAsync(x));
            }
            else
            {
                var user = selectionMenu.Channel.GetGuild().GetUser(selectionMenu.User.Id);
                user.RemoveRolesAsync(roles.Where(user.Roles.Contains));
            }

            selectionMenu.RespondAsync();
        }
        switch (id)
        {
            case 0:
                RoleReplace(GetColorRoles((SocketTextChannel)selectionMenu.Channel));
                break;
            case 1:
                RoleReplace(GetPronounRoles((SocketTextChannel)selectionMenu.Channel));
                break;
            case 2:
                RoleReplace(GetPingRoles((SocketTextChannel)selectionMenu.Channel), false);
                break;
            case 3:
                RoleReplace(GetBotFunRoles((SocketTextChannel)selectionMenu.Channel), false);
                break;
            case 4:
                RoleReplace(GetArtSpecialityRoles((SocketTextChannel)selectionMenu.Channel), false);
                break;
            case 5:
                RoleReplace(GetMiscRoles((SocketTextChannel)selectionMenu.Channel), false);
                break;
        }
    }
}