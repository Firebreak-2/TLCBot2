using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task ThreadDeleted() =>
        Program.Client.ThreadDeleted += async oldThreadCache =>
        {
            const string name = nameof(DiscordSocketClient.ThreadDeleted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.ThreadDelete).ToArrayAsync();
            var log = auditLogs[0].First();
            var logData = (ThreadDeleteAuditLogData) log.Data;
            var user = log.User;

            var oldThread = await oldThreadCache.GetOrDownloadAsync();

            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] deleted thread [{logData.ThreadName}]",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id),
                    ("thread", logData.ThreadId),
                    ("channel", (oldThread?.ParentChannel?.Id.ToString()).EnsureString())
                )),
                logData.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("ThreadDeleted".Titleize())
                .AddField("Thread", $"<#{logData.ThreadId}>")
                .AddField("Details", logData.ToJson().ToCodeBlock("json"))
                .AddField("Deleted By", user.Mention)
            );
        };
}