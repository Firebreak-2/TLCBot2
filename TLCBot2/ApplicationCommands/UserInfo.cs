using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Data;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("user-info", "Shows general information about the selected user")]
    [UserCommand("User Info")]
    public async Task UserInfo(SocketGuildUser user)
    {
        await using var db = new TlcDbContext();

        int cookies = 0;
        if (await db.Users.FindAsync(user.Id) is { } entry)
            cookies = entry.Balance;

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithAuthor(user)
            .AddField("Server Join Date", $"<t:{user.JoinedAt!.Value.ToUnixTimeSeconds()}>")
            .AddField("Account Creation Date", $"<t:{user.CreatedAt.ToUnixTimeSeconds()}>")
            .AddField("Cookies", cookies)
            .AddField("User ID", user.Id);
        
        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}