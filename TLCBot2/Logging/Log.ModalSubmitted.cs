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
    public static async Task ModalSubmitted() =>
        Program.Client.ModalSubmitted += async modal =>
        {
            const string name = nameof(DiscordSocketClient.ModalSubmitted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var fields = new Dictionary<string, object>
            {
                {"Custom ID", modal.Data.CustomId},
                {"Components", modal.Data.Components
                        .ToDictionary(
                            x => x.CustomId,
                            x => new
                            {
                                Value = x.Value.ToString(),
                                Type = x.Type.Humanize()
                            })
                },
            };
            
            var logEntry = new LogEntry(name, importance, 
                $"[{modal.User.Username}] submitted modal",
                tags.Select(x => x.MappedFormat(
                    ("user", modal.User.Id),
                    ("channel", (modal.ChannelId?.ToString()).EnsureString())
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("ModalSubmitted".Titleize())
                .AddField("Details", fields.ToJson().ToCodeBlock("json")));
        };
}