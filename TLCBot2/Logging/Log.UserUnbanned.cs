using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Data.StringPrompts;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task UserUnbanned() =>
        Program.Client.UserUnbanned += async (user, guild) =>
        {
            if (guild.Id != RuntimeConfig.FocusServer?.Id)
                return;

            const string name = nameof(DiscordSocketClient.UserUnbanned);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var auditLogs = await RuntimeConfig.FocusServer.GetAuditLogsAsync(1, actionType: ActionType.Unban).ToArrayAsync();
            var firstLog = auditLogs[0].First();
            
            var kicker = firstLog.User;
            string reason = firstLog.Reason.EnsureString();

            var fields = new Dictionary<string, object>
            {
                {"User", user.Id},
                {"Created At", user.CreatedAt.ToUnixTimeSeconds()},
                {"Unbanned By", kicker.Id},
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
                .WithTitle("User Unbanned")
                .AddField("User", user.Mention)
                .AddField("Created At", user.CreatedAt.ToDynamicTimestamp())
                .AddField("Unbanned By", kicker.Mention)
                .AddField("Reason", reason));
        }; 
}