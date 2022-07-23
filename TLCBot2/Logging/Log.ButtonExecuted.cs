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
    public static async Task ButtonExecuted() =>
        Program.Client.ButtonExecuted += async component =>
        {
            const string name = nameof(DiscordSocketClient.ButtonExecuted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var button = component.Message.GetSpecificComponent<ButtonComponent>(component.Data.CustomId);

            var fields = new Dictionary<string, object>
            {
                {"Custom ID", component.Data.CustomId.EnsureString()},
                {"Label", button.Label},
                {"Emote", (button.Emote?.Name).EnsureString()},
                {"Style", button.Style.ToString()},
                {"URL", button.Url.EnsureString()},
            };
            
            var logEntry = new LogEntry(name, importance, 
                $"[{component.User.Username}] pressed a button [{component.Data.CustomId}]",
                tags.Select(x => x.MappedFormat(
                    ("channel", component.Message.Channel.Id),
                    ("user", component.User.Id),
                    ("message", component.Message.Id),
                    ("customId", component.Data.CustomId),
                    ("cType", component.Type.ToString())
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Button Pressed")
                .AddField("Pressed By", component.User.Mention)
                .AddField("Button Label", button.Label)
                .AddField("Button ID", component.Data.CustomId)
            );
        };
}