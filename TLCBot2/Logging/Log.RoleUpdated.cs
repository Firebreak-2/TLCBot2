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
    public static async Task RoleUpdated() =>
        Program.Client.RoleUpdated += async (oldRole, newRole) =>
        {
            const string name = nameof(DiscordSocketClient.RoleUpdated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.RoleUpdated).ToArrayAsync();

            var user = auditLogs[0].First().User;

            var oldFields = new Dictionary<string, object>
            {
                {"Name", oldRole.Name},
                {"Color", oldRole.Color.ToString()},
                {"Icon", oldRole.GetIconUrl()},
            };

            var newFields = new Dictionary<string, object>
            {
                {"Name", newRole.Name},
                {"Color", newRole.Color.ToString()},
                {"Icon", newRole.GetIconUrl()},
            };

            string oldFieldsJson = oldFields.ToJson();
            string newFieldsJson = newFields.ToJson();
            if (oldFieldsJson == newFieldsJson)
                return;
            
            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] updated role [{newRole.Id}]",
                tags.Select(x => x.MappedFormat(
                    ("role", newRole.Id),
                    ("user", user.Id)
                )),
                new Dictionary<string, Dictionary<string, object>>(2)
                {
                    {"Old Fields", oldFields},
                    {"New Fields", newFields},
                }.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed =>
                {
                    embed
                        .WithTitle("Role Updated")
                        .AddField("Previous", oldFieldsJson.ToCodeBlock("json"))
                        .AddField("Current", newFieldsJson.ToCodeBlock("json"))
                        .AddField("Role ID", newRole.Id)
                        .AddField("Updated By", user.Mention);

                    if ((string) newFields["Icon"] 
                        is var icon 
                        and (not null or {Length: 0}))
                        embed.WithThumbnailUrl(icon);
                    
                    return embed;
                }
            );
        };
}