using Discord;
using Discord.Rest;
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
    public static async Task RoleDeleted() =>
        Program.Client.RoleDeleted += async role =>
        {
            const string name = nameof(DiscordSocketClient.RoleDeleted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.RoleDeleted).ToArrayAsync();

            var user = auditLogs[0].First().User;

            var fields = new Dictionary<string, object>
            {
                {"Name", role.Name},
                {"Color", role.Color.ToString()},
                {"ID", role.Id},
                {"Icon", role.GetIconUrl()},
            };
            
            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] deleted role [{role.Id}]",
                tags.Select(x => x.MappedFormat(
                    ("role", role.Id),
                    ("user", user.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed =>
                {
                    embed
                        .WithTitle("Role Deleted")
                        .AddField("Name", fields["Name"])
                        .AddField("Color", fields["Color"])
                        .AddField("Deleted By", user.Mention);

                    if ((string) fields["Icon"] 
                        is var icon 
                        and (not null or {Length: 0}))
                        embed.WithThumbnailUrl(icon);
                    
                    return embed;
                }
            );
        };
}