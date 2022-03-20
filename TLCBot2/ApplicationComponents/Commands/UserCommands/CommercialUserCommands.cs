using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.DataManagement;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents.Commands.UserCommands;

public static class CommercialUserCommands
{
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;
        const bool devOnly = false;

        #region User Info Command
        await FireUserCommand.CreateNew(new FireUserCommand(new UserCommandBuilder()
            .WithName("User Info"), cmd =>
        {
            var user = cmd.Channel.GetGuild().GetUser(cmd.Data.Member.Id);
            int cookies = 0;
            if (CookieManager.GetUser(user.Id, out var entry))
                cookies = entry.Cookies;

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithAuthor(user)
                .AddField("Server Join Date", $"<t:{user.JoinedAt!.Value.ToUnixTimeSeconds()}>")
                .AddField("Account Creation Date", $"<t:{user.CreatedAt.ToUnixTimeSeconds()}>")
                // .AddField("Server Permissions", 
                //     string.Join("\n", user.GuildPermissions.ToList()
                //         .Select(x => x.ToString())))
                .AddField("Cookies", cookies)
                .AddField("Is Cookie Banned", entry?.IsBanned ?? false ? "Yes" : "No")
                .AddField("User ID", user.Id);
            cmd.RespondAsync(embed:embed.Build(), ephemeral: true);
        }, devOnly), guild);
        #endregion
    }
}