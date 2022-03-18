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

        #region Test Command

        await FireUserCommand.CreateNew(new FireUserCommand(new UserCommandBuilder()
            .WithName("cookies"), cmd =>
        {
            var user = cmd.Data.Member;
            
            int cookies = 0;
            if (CookieManager.GetUser(user.Id, out var entry))
                cookies = entry.Cookies;

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle($"{user.Username}'s balance: {cookies}");
                
            cmd.RespondAsync(embed:embed.Build(), ephemeral: true);
        }), guild);

        #endregion
    }
}