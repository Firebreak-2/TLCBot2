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
    public static async Task ReactionRemoved() =>
        Program.Client.ReactionRemoved += async (messageCache, _, reaction) =>
        {
            IUserMessage? message = await messageCache.GetOrDownloadAsync();
            IMessageChannel? channel = reaction.Channel;
            
            const string name = nameof(DiscordSocketClient.ReactionRemoved);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var logEntry = new LogEntry(name, importance, 
                $"[{reaction.User.GetValueOrDefault(null)?.Username.EnsureString()}] removed their reaction of [{reaction.Emote.Name}] from message [{message?.Id.ToString().EnsureString()}]", 
                tags.Select(x => x.MappedFormat(
                    ("channel", channel.Id),
                    ("user", reaction.UserId),
                    ("message", (message?.Id).ToString().EnsureString()),
                    ("emote", reaction.Emote.GetEmoteID()))),
                new Dictionary<string, object>
                {
                    {"Emote", reaction.Emote.Name},
                    {"Channel", channel.Id},
                    {"Message", reaction.MessageId},
                    {"Reactor", reaction.UserId},
                }.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Reaction Removed")
                .AddField("Reactor", reaction.User.GetValueOrDefault(null)?.Mention ?? "Unable to fetch reactor")
                .AddField("Reaction Emote", reaction.Emote.Name)
                .AddField("Message", $"{message?.Id.ToString().EnsureString()} - {message?.GetJumpHyperLink() ?? "Unable to fetch message"}")
            );
        };
}