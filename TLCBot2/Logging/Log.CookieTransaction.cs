using Discord.WebSocket;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{
    public static async Task CookieTransaction(SocketUser? changedBy, SocketUser user, int oldBalance, int newBalance,
        string? reason = null)
    {
        const string name = "UserBalanceUpdated";
        (Importance importance, string[] tags) = LoggingEventData.All[name];

        var logEntry = new LogEntry(name, Importance.Useless,
            $"Balance updated for [{user}]. [{oldBalance}] -> [{newBalance}]",
            tags.Select(x => x.MappedFormat(
                ("user", changedBy?.Id.ToString() ?? "SYSTEM"),
                ("targetUser", user.Id))),
            new Dictionary<string, object>
            {
                {"Old Balance", oldBalance},
                {"New Balance", newBalance},
                {"Reason", reason ?? "No reason provided"},
            }.ToJson());
        
        await ToFile(logEntry);
        
        await ToChannel(logEntry, embed => embed
            .WithTitle("Cookies Changed")
            .AddField("User", user.Mention)
            .AddField("Old Balance", oldBalance)
            .AddField("New Balance", newBalance)
            .AddField("Reason", reason ?? "No reason provided")
            .AddField("By", changedBy?.Mention ?? "SYSTEM"));
    }
}