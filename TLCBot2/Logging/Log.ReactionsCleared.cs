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
    public static async Task ReactionsCleared() =>
        Program.Client.ReactionsCleared += async (messageCache, channelCache) =>
        {
            IUserMessage? message = await messageCache.GetOrDownloadAsync();
            IMessageChannel? channel = await channelCache.GetOrDownloadAsync();
            
            const string name = nameof(DiscordSocketClient.ReactionsCleared);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var logEntry = new LogEntry(name, importance, 
                $"Reactions cleared for message [{message?.Id.ToString().EnsureString()}] in channel [{channel.Id}]",
                tags.Select(x => x.MappedFormat(
                    ("channel", channel.Id),
                    ("message", (message?.Id).ToString().EnsureString())
                    )),
                new Dictionary<string, object>
                {
                    {"Channel", channel.Id},
                    {"Message", (message?.Id).ToString().EnsureString()},
                }.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Reactions Cleared")
                .AddField("Message", $"{message?.Id.ToString().EnsureString()} - {message?.GetJumpHyperLink() ?? "Unable to fetch message"}")
            );
        };
}