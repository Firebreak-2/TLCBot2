using Discord.WebSocket;

namespace TLCBot2.Logging;

public static partial class Log
{
    public static async Task CookieTransaction(SocketGuildUser user, int oldBalance, int newBalance,
        string? reason = null)
    {
        await ToFile();
        await ToChannel(Importance.Useless, 
            builder => builder
            .WithTitle("Cookies Changed")
            .AddField("User", user.Mention)
            .AddField("Old Balance", oldBalance)
            .AddField("New Balance", newBalance)
            .AddField("Reason", reason ?? "No reason provided"));
    }
}