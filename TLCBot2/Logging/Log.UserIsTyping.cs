using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task UserIsTyping() =>
        Program.Client.UserIsTyping += async (userCache, channelCache) =>
        {
            var user = await userCache.GetOrDownloadAsync();
            var channel = await channelCache.GetOrDownloadAsync();

            if (user is null || channel is null)
                return;
        
            const string name = nameof(DiscordSocketClient.UserIsTyping);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            
            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] is typing in channel [{channel.Id}]",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id),
                    ("channel", channel.Id)
                )),
                "{}");

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("User Is Typing")
                .AddField("User", user.Mention)
                .AddField("Channel", $"<#{channel.Id}>"));
        };
}