using Discord;
using Discord.Commands;
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
    public static async Task MessageDeleted() =>
        Program.Client.MessageDeleted += async (messageCache, channelCache) =>
        {
            var message = (SocketMessage?) await messageCache.GetOrDownloadAsync();
            var channel = await channelCache.GetOrDownloadAsync();
            
            const string name = nameof(DiscordSocketClient.MessageDeleted);
            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.MessageDeleted)
                .ToArrayAsync();

            var firstLog = auditLogs[0].First();
            var now = DateTimeOffset.Now;
            const int leeway = 5;
            IUser? user = firstLog.CreatedAt.Second + leeway >= now.Second
                ? firstLog.User 
                : message?.Author;
            
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var logEntry = new LogEntry(name, importance, $"[{user?.Username.EnsureString()}] deleted message sent by [{message?.Author.Username.EnsureString()}]",
                tags.Select(x => x.MappedFormat(
                        ("channel", (channel?.Id).ToString().EnsureString()),
                        ("message", (message?.Id).ToString().EnsureString()),
                        ("user", (user?.Id).ToString().EnsureString()),
                        ("targetUser", (message?.Author.Id).ToString().EnsureString()))),
                (message?.GetMessageDetails() ?? null).ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed =>
            {
                embed.WithTitle("Message Deleted");

                if (message is not null)
                {
                    if (message.Content.Length > 0)
                        embed.AddField("Content", $"{(message.Content.Length > 1000 ? "Text too long to preview" : message.Content)}".ToCodeBlock());

                    embed
                        .AddField("Author", message.Author.Mention);
                        
                    if (string.Join('\n', message.Attachments.Select(x => $"<{x.Url}>"))
                        is {Length: > 0} t1)
                        embed.AddField("Attachments", t1);
                    if (string.Join('\n', message.Reactions.Select(x => $":{x.Key.Name}: {x.Value.ReactionCount}"))
                        is {Length: > 0} t2)
                        embed.AddField("Reactions", t2);

                    if (message.Attachments.Count > 0)
                        embed.WithImageUrl(message.Attachments.First().Url);
                }

                if (user is not null)
                {
                    embed.WithAuthor(user);
                }

                return embed
                    .AddField("In Channel", $"<#{channel?.Id ?? 0}>")
                    .AddField("[uncertain] Deleted By", $"{user?.Mention.EnsureString()}");
            });
        };
}