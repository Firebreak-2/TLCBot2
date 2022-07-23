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
    public static async Task GuildUpdated() =>
        Program.Client.GuildUpdated += async (oldGuild, newGuild) =>
        {
            if (newGuild.Id != RuntimeConfig.FocusServer?.Id)
                return;
            
            const string name = nameof(DiscordSocketClient.GuildUpdated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var auditLogs = await RuntimeConfig.FocusServer.GetAuditLogsAsync(1, actionType: ActionType.GuildUpdated).ToArrayAsync();
            var user = auditLogs[0].First().User;

            var oldFields = GetDetails(oldGuild);
            var newFields = GetDetails(newGuild);
            
            string oldFieldsJson = oldFields.ToJson();
            string newFieldsJson = newFields.ToJson();

            if (oldFieldsJson == newFieldsJson)
                return;
            
            var logEntry = new LogEntry(name, importance,
                $"[{user.Username}] updated the guild",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id)
                )),
                new Dictionary<string, object>(2)
                {
                    {"Previous Details", oldFields},
                    {"Current Details", newFields}
                }.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed =>
                {
                    embed.WithTitle("Server Updated");

                    foreach (string key in oldFields.Keys)
                    {
                        string o = (oldFields.GetValueOrDefault(key)?.ToString()).EnsureString();
                        string n = (newFields.GetValueOrDefault(key)?.ToString()).EnsureString();
                        if (o != n)
                            embed.AddField(key, $"**Previous**{o.ToCodeBlock("")}\n**Current**{n.ToCodeBlock("")}");
                    }
                    
                    return embed.AddField("Updated By", user.Mention);
                }
            );

            static Dictionary<string, object> GetDetails(SocketGuild oldGuild) =>
                new()
                {
                    {"Name", oldGuild.Name},
                    {"Vanity URL Code", oldGuild.VanityURLCode},
                    {"Boost Tier", oldGuild.PremiumTier},
                    {"Has Boost Progress Bar", oldGuild.IsBoostProgressBarEnabled},
                    {"Description", oldGuild.Description},
                    {"Owner", oldGuild.Owner.Id},
                    {"Icon", oldGuild.IconUrl},
                    {"Banner", oldGuild.BannerUrl},
                    {"System Channel", oldGuild.SystemChannel.Id}
                };
        };
}