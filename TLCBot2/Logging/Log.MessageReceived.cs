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
    public static async Task MessageReceived() =>
        Program.Client.MessageReceived += async message =>
        {
            if (message.Author.IsBot)
                return;
            
            const string name = nameof(DiscordSocketClient.MessageReceived);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            
            var logEntry = new LogEntry(name, importance, 
                $"[{message.Author.Username}] sent a message in channel [{message.Channel.Id}]",
                tags.Select(x => x.MappedFormat(
                    ("user", message.Author.Id),
                    ("channel", message.Channel.Id),
                    ("message", message.Id)
                )),
                $"{{ \"Content\": \"{message.Content}\" }}");

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Message Received")
                .AddField("User", message.Author.Mention)
                .AddField("Channel", $"<#{message.Channel.Id}>")
                .AddField("Message", $"{message.Id} - {message.GetJumpHyperLink()}"));
        };
}