using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task InviteDeleted() =>
        Program.Client.InviteDeleted += async (guild, inviteCode) =>
        {
            if (guild.Id != RuntimeConfig.FocusServer?.Id)
                return;
            
            const string name = nameof(DiscordSocketClient.InviteDeleted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];
            var auditLogs = await RuntimeConfig.FocusServer.GetAuditLogsAsync(1, actionType: ActionType.InviteDeleted).ToArrayAsync();

            var user = auditLogs[0].First().User;

            var fields = new Dictionary<string, object>
            {
                {"Invite Code", inviteCode},
            };

            await using (var db = new TlcDbContext())
            {
                if (await db.ServerInvites.FindAsync(inviteCode) is { } invite)
                {
                    fields.Add("Times Used", invite.Uses);
                    
                    db.ServerInvites.Remove(invite);
                    await db.SaveChangesAsync();
                }
            }
            
            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] deleted an invite [discord.gg/{inviteCode}]",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Invite Created")
                .AddField("Invite Link", $"https://discord.gg/{inviteCode}")
                .AddField("Times Used", fields.GetValueOrDefault("Times Used")?.ToString().EnsureString("???"))
                .AddField("Deleted By", user.Mention)
            );
        };
}