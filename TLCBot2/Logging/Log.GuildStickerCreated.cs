using Discord;
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
    public static async Task GuildStickerCreated() =>
        Program.Client.GuildStickerCreated += async sticker =>
        {
            const string name = nameof(DiscordSocketClient.GuildStickerCreated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.StickerCreated).ToArrayAsync();
            var user = auditLogs[0].First().User;

            var fields = new Dictionary<string, string>
            {
                {"Name", sticker.Name},
                {"Sticker URL", sticker.GetStickerUrl()},
                {"Description", sticker.Description},
            };
            
            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] created a new sticker [{sticker.Name}]",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Sticker Created")
                .AddField("Name", fields["Name"])
                .AddField("Description", fields["Description"])
                .AddField("Created By", user.Mention)
                .WithImageUrl(fields["Sticker URL"]));
        };
}