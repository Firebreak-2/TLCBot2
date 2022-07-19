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
    public static async Task GuildStickerUpdated() =>
        Program.Client.GuildStickerUpdated += async (oldSticker, newSticker) =>
        {
            const string name = nameof(DiscordSocketClient.GuildStickerUpdated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.StickerDeleted).ToArrayAsync();
            var user = auditLogs[0].First().User;

            var oldFields = new Dictionary<string, string>
            {
                {"Name", oldSticker.Name},
                {"Sticker URL", oldSticker.GetStickerUrl()},
                {"Description", oldSticker.Description},
            };

            var newFields = new Dictionary<string, string>
            {
                {"Name", newSticker.Name},
                {"Sticker URL", newSticker.GetStickerUrl()},
                {"Description", newSticker.Description},
            };
            
            var fields = oldFields.Keys
                .Where(x => oldFields[x].ToJson() != newFields[x].ToJson())
                .ToDictionary(x => x, x => new Dictionary<string, object>(2)
                {
                    {"Previous", oldFields[x]},
                    {"Current", newFields[x]},
                });

            if (!fields.Any())
                return;
            
            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] updated a sticker [{newSticker.Name}]",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Sticker Updated")
                .WithFields(fields.Select(x => new EmbedFieldBuilder()
                    .WithName(x.Key)
                    .WithValue(x.Value.ToJson().ToCodeBlock("json"))))
                .AddField("Updated By", user.Mention)
                .WithImageUrl(newFields["Sticker URL"]));
        };
}