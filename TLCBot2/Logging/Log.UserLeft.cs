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
    public static async Task UserLeft() =>
        Program.Client.UserLeft += async (guild, user) =>
        {
            if (guild.Id != RuntimeConfig.FocusServer?.Id)
                return;

            const string name = nameof(DiscordSocketClient.UserLeft);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var auditLogs = await RuntimeConfig.FocusServer.GetAuditLogsAsync(1, actionType: ActionType.Kick).ToArrayAsync();
            var firstLog = auditLogs[0].First();
            
            var kicker = firstLog.User;
            string reason = firstLog.Reason.EnsureString();

            var fields = new Dictionary<string, object>
            {
                {"User", user.Id},
                {"Created At", user.CreatedAt.ToUnixTimeSeconds()},
                {"Kicked By", (kicker?.Id).ToString().EnsureString()},
                {"Reason", reason},
            };
            
            var logEntry = new LogEntry(name, importance,
                kicker is not null
                    ? $"[{kicker.Username}] kicked user [{user.Username}]"
                    : $"[{user.Username}] left the server",
                tags.Select(x => x.MappedFormat(
                    ("user", (kicker?.Id).ToString().EnsureString()),
                    ("targetUser", user.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("User Left")
                .AddField("User", user.Mention)
                .AddField("Created At", user.CreatedAt.ToDynamicTimestamp())
                .AddField("Kicked By", (kicker?.Mention).EnsureString())
                .AddField("Reason", reason));
        }; 
}