using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task UserBanned() =>
        Program.Client.UserBanned += async (user, guild) =>
        {
            if (guild.Id != RuntimeConfig.FocusServer?.Id)
                return;

            const string name = nameof(DiscordSocketClient.UserBanned);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var auditLogs = await RuntimeConfig.FocusServer.GetAuditLogsAsync(1, actionType: ActionType.Ban).ToArrayAsync();
            var firstLog = auditLogs[0].First();
            
            var kicker = firstLog.User;
            string reason = firstLog.Reason.EnsureString();

            var fields = new Dictionary<string, object>
            {
                {"User", user.Id},
                {"Created At", user.CreatedAt.ToUnixTimeSeconds()},
                {"Banned By", kicker.Id},
                {"Reason", reason},
            };
            
            var logEntry = new LogEntry(name, importance,
                $"[{kicker.Username}] banned user [{user.Username}]",
                tags.Select(x => x.MappedFormat(
                    ("user", kicker.Id),
                    ("targetUser", user.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("User Banned")
                .AddField("User", user.Mention)
                .AddField("Created At", user.CreatedAt.ToDynamicTimestamp())
                .AddField("Banned By", kicker.Mention)
                .AddField("Reason", reason));
        }; 
}