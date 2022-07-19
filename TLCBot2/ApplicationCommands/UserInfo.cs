using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Data;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("user-info", "Shows general information about the selected user")]
    [UserCommand("User Info")]
    [EnabledInDm(false)]
    public async Task UserInfo(SocketGuildUser? user = null)
    {
        user ??= (SocketGuildUser) Context.User;
        
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
            .AddField("User ID", user.Id)
            .AddField("Active Clients", string.Join('\n', user.ActiveClients.Select(x => $"{x} Client"))
                                        is {Length: > 0} t1 ? t1 : "None")
            .AddField("Status", user.Status);
        
        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}