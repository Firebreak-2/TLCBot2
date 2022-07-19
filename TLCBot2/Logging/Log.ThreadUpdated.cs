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
    public static async Task ThreadUpdated() =>
        Program.Client.ThreadUpdated += async (_, newThread) =>
        {
            const string name = nameof(DiscordSocketClient.ThreadUpdated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.ThreadUpdate).ToArrayAsync();
            var log = auditLogs[0].First();
            var logData = (ThreadUpdateAuditLogData) log.Data;
            var user = log.User;

            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] updated thread [{newThread.Name}]",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id),
                    ("thread", newThread.Id),
                    ("channel", newThread.ParentChannel.Id)
                )),
                new Dictionary<string, ThreadInfo>(2)
                {
                    {"Previous", logData.Before},
                    {"Current", logData.After},
                }.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("ThreadUpdated".Titleize())
                .AddField("Thread", newThread.Mention)
                .AddField("Previous Details", logData.Before.ToJson().ToCodeBlock("json"))
                .AddField("Current Details", logData.After.ToJson().ToCodeBlock("json"))
                .AddField("Updated By", user.Mention)
            );
        };
}