using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task ReactionsRemovedForEmote() =>
        Program.Client.ReactionsRemovedForEmote += async (messageCache, channelCache, emote) =>
        {
            IUserMessage? message = await messageCache.GetOrDownloadAsync();
            IMessageChannel? channel = await channelCache.GetOrDownloadAsync();
            
            const string name = nameof(DiscordSocketClient.ReactionsRemovedForEmote);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var logEntry = new LogEntry(name, importance, 
                $"Reactions with emote [{emote.GetEmoteID()}] cleared for message [{message?.Id.ToString() ?? "_"}] in channel [{channel.Id}]",
                tags.Select(x => x.MappedFormat(
                    ("channel", channel.Id),
                    ("message", message?.Id.ToString() ?? "_"),
                    ("emote", emote.GetEmoteID())
                )),
                new Dictionary<string, object>
                {
                    {"Channel", channel.Id},
                    {"Message", message?.Id.ToString() ?? "_"},
                    {"emote", emote.GetEmoteID()}
                }.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Reactions Removed For Emote")
                .AddField("Message", $"{message?.Id.ToString() ?? "_"} - {message?.GetJumpHyperLink() ?? "Unable to fetch message"}")
                .AddField("Reaction Emote", $"{emote.Name} [{emote.GetEmoteID()}]")
            );
        };
}