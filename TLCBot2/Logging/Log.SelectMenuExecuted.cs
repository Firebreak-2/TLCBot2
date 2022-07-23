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
    public static async Task SelectMenuExecuted() =>
        Program.Client.SelectMenuExecuted += async component =>
        {
            const string name = nameof(DiscordSocketClient.SelectMenuExecuted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var selectMenu = component.Message.GetSpecificComponent<SelectMenuComponent>(component.Data.CustomId);

            var fields = new Dictionary<string, object>
            {
                {"Custom ID", component.Data.CustomId.EnsureString()},
                {"Placeholder", selectMenu.Placeholder.EnsureString()},
                {"SelectedOptions", component.Data.Values},
                {"AllOptions", selectMenu.Options.ToDictionary(x => x.Label, x => new
                {
                    x.Value,
                    EmoteName = x.Emote?.Name.EnsureString(),
                    x.Description,
                })},
                {"InputRange", $"{selectMenu.MinValues}-{selectMenu.MaxValues}"},
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
                .WithTitle("Select Menu Submitted")
                .AddField("Submitted By", component.User.Mention)
                .AddField("Select Menu Placeholder", selectMenu.Placeholder.EnsureString())
                .AddField("Selected Options", component.Data.Values.ToJson().ToCodeBlock("json"))
                .AddField("Select Menu ID", component.Data.CustomId)
            );
        };
}