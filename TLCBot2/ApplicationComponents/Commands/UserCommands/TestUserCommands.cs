using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.DataManagement;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents.Commands.UserCommands;

public static class TestUserCommands
{
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;

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
        }), guild);
        #endregion

        #region Give Cookies Command
        await FireUserCommand.CreateNew(new FireUserCommand(new UserCommandBuilder()
            .WithName("Give Cookies"), cmd =>
        {
            cmd.RespondWithModalAsync(FireModal.CreateNew(new FireModal(
                new ModalBuilder("Give Cookies", $"modal-{Helper.RandomInt(0, 9999)}")
                    .AddTextInput("How many to give? (or take with negatives)", "tb-0", value: "5")
                    .AddTextInput("What was the reason?", "tb-1", required: false, style: TextInputStyle.Paragraph)
                , modal =>
                {
                    SocketUser user = cmd.Data.Member;
                    int count = int.Parse(modal.Data.Components.First(x => x.CustomId == "tb-0").Value);
                    string reason = modal.Data.Components.Count == 2 
                        ? modal.Data.Components.First(x => x.CustomId == "tb-1").Value
                        : "No reason given";
                    
                    CookieManager.TakeOrGiveCookiesToUser(user.Id, count, reason);
                    CookieManager.GetUser(user.Id, out var entry);

                    bool isPositive = count >= 0;
            
                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle($"{(isPositive ? "Given" : "Taken")} `{Math.Abs(count)}` 🍪 {(isPositive ? "to" : "from")} {user.Username}.")
                        .WithDescription($"current balance: {entry.Cookies}");
            
                    modal.RespondAsync(embed:embed.Build(), ephemeral: true);
                })));
        }, true), guild);
        #endregion
    }
}