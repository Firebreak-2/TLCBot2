using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task InteractionCreated() =>
        Program.Client.InteractionCreated += async interaction =>
        {
            const string name = nameof(DiscordSocketClient.InteractionCreated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            
            var fields = new Dictionary<string, object>
            {
                {"Type", interaction.Type.ToString()},
                {"Channel", (interaction.ChannelId?.ToString()).EnsureString()},
            };
            
            var logEntry = new LogEntry(name, importance,
                $"Interaction created for [{interaction.User.Username}]",
                tags.Select(x => x.MappedFormat(
                    ("user", interaction.User.Id),
                    ("channel", (interaction.ChannelId?.ToString()).EnsureString())
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed =>
                {
                    embed
                        .WithTitle("Interaction Created")
                        .AddField("Interaction Type", interaction.Type.ToString())
                        .AddField("For User", interaction.User.Mention);

                    if (interaction.ChannelId is { } id)
                        embed.AddField("Channel", $"<#{id}>");
                    
                    return embed;
                }
            );
        };
}