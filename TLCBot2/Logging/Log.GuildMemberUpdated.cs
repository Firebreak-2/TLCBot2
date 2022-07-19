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
    public static async Task GuildMemberUpdated() =>
        Program.Client.GuildMemberUpdated += async (oldUserCache, newUser) =>
        {
            const string name = nameof(DiscordSocketClient.GuildMemberUpdated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.MemberRoleUpdated)
                .ToArrayAsync();

            var firstLog = auditLogs[0].First();
            
            var now = DateTimeOffset.Now;
            const int leeway = 5;
            MemberRoleAuditLogData? data = firstLog.CreatedAt.Second + leeway >= now.Second
                ? (MemberRoleAuditLogData) firstLog.Data
                : null;
            
            var oldUser = await oldUserCache.GetOrDownloadAsync();
            
            Dictionary<string, object>? oldFields = oldUser is null 
                ? null 
                : new Dictionary<string, object>
                  {
                      {"Username", oldUser.Nickname},
                      {"Nickname", oldUser.Username},
                  };
            Dictionary<string, object> newFields = new()
                {
                    {"Username", newUser.Nickname},
                    {"Nickname", newUser.Username},
                };
            
            var roleChanges = data?.Roles.ToDictionary(
                x => $"{x.RoleId}",
                x => new Dictionary<string, object>(2)
                {
                    {"Name", x.Name},
                    {"Added", x.Added}
                });

            var fields = new Dictionary<string, object>(3);
            if (oldFields is not null
                && oldFields.ToJson() is var oF
                && newFields.ToJson() is var nF
                && oF != nF)
            {
                fields.Add("Previous Details", oldFields);
                fields.Add("Current Details", newFields);
            }

            if (roleChanges is { } rC)
            {
                fields.Add("Roles Updated", rC);
            }

            if (!fields.Any())
                return;
            
            var logEntry = new LogEntry(name, importance, 
                $"Guild Member Updated [{newUser.Username}]",
                tags.Select(x => x.MappedFormat(
                    ("user", newUser.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => 
                embed
                    .WithTitle("Guild Member Updated")
                    .WithFields(fields.Select(x => new EmbedFieldBuilder()
                        .WithName(x.Key)
                        .WithValue(x.Value.ToJson().ToCodeBlock("json"))
                    ))
                );
        };
}