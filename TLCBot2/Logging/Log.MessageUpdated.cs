using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{
    [PreInitialize]
    public static async Task MessageUpdated() =>
        Program.Client.MessageUpdated += async (oldMessageCache, newMessage, channel) =>
        {
            if (newMessage.Author.IsBot)
                return;
            
            const string name = nameof(DiscordSocketClient.MessageUpdated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var oldMessage = await oldMessageCache.GetOrDownloadAsync();

            Dictionary<string, object> fields = new();
            
            string oldContent = "Unable to fetch old message content";
            if (oldMessage is not null)
                oldContent = oldMessage.Content;

            if (oldContent == newMessage.Content)
                return;

            fields.Add("Previous Content",
                $"{(oldContent.Length > 1000 ? "Text too long to preview" : oldContent)}");
            fields.Add("Current Content", 
                $"{(newMessage.Content.Length > 1000 ? "Text too long to preview" : newMessage.Content)}");
            if (string.Join('\n', newMessage.Attachments.Select(x => $"<{x.Url}>"))
                is {Length: > 0} t1)
                fields.Add("Attachments", t1);
            if (string.Join('\n', newMessage.Reactions.Select(x => $":{x.Key.Name}: {x.Value.ReactionCount}"))
                is {Length: > 0} t2)
                fields.Add("Reactions", t2);
            fields.Add("In Channel", channel.Id);
            fields.Add("Author", newMessage.Author.Id);

            var logEntry = new LogEntry(name, importance,
                $"[{newMessage.Author.Username}] edited their message [{newMessage.Id}]",
                tags.Select(x => x.MappedFormat(
                    ("channel", channel.Id),
                    ("message", newMessage.Id),
                    ("user", newMessage.Author.Id))),
                fields.ToJson());
                    
            await ToChannel(logEntry, embed =>
            {
                embed.WithTitle("Message Updated");

                foreach ((string field, object? value) in fields)
                {
                    // sorry bad code but too lazy to fix, i
                    // just wanna get it working for now :P
                    
                    string val = value?.ToString() ?? "null";
                    embed.AddField(field, field.Contains("Content") 
                        ? val.ToCodeBlock() 
                        : field.Contains("Channel") 
                            ? $"<#{val}> - {newMessage.GetJumpHyperLink()}"
                            : field == "Author" 
                                ? $"<@!{val}>"
                                : val);
                }

                return embed.WithAuthor(newMessage.Author);
            });
            
            await ToFile(logEntry);
        };
}