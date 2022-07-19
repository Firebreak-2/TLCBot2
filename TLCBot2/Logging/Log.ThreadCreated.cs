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
    public static async Task ThreadCreated() =>
        Program.Client.ThreadCreated += async thread =>
        {
            const string name = nameof(DiscordSocketClient.ThreadCreated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.ThreadCreate).ToArrayAsync();
            var log = auditLogs[0].First();
            var user = log.User;

            var fields = new Dictionary<string, object>
            {
                {"Name", thread.Name},
                {"Owner", thread.Owner.Id},
                {"Is Private", thread.IsPrivateThread},
                {"Auto Archive Duration", thread.AutoArchiveDuration.Humanize()},
            };
            
            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] created thread [{thread.Name}]",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id),
                    ("thread", thread.Id),
                    ("channel", thread.ParentChannel.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("ThreadCreated".Titleize())
                .AddField("Thread", thread.Mention)
                .AddField("Created By", user.Mention));
        };
}