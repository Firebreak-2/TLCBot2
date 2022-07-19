using Discord;
using Discord.Rest;
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
    public static async Task MessagesBulkDeleted() =>
        Program.Client.MessagesBulkDeleted += async (messageCacheCollection, channelCache) =>
        {
            var channel = await channelCache.GetOrDownloadAsync();
            
            const string name = nameof(DiscordSocketClient.MessagesBulkDeleted);
            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.MessageBulkDeleted)
                .ToArrayAsync();

            IUser user = auditLogs[0].First().User;
            
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] deleted multiple messages [{messageCacheCollection.Count}] in channel [{channel?.Id.ToString().EnsureString()}]",
                tags.Select(x => x.MappedFormat(
                        ("channel", (channel?.Id).ToString().EnsureString()),
                        ("user", user.Id))),
                new Dictionary<string, object>(2)
                {
                    { "Amount", messageCacheCollection.Count },
                    { "Messages", messageCacheCollection
                                    .Select(async x => (await x.GetOrDownloadAsync()).GetMessageDetails())
                                    .ToArray() },
                }.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Many Messages Deleted")
                .AddField("Amount", messageCacheCollection.Count)
                .AddField("In Channel", $"<#{channel?.Id ?? 0}>")
                .AddField("[uncertain] Deleted By", $"{user.Mention.EnsureString()}")
                .WithAuthor(user));
        };
}