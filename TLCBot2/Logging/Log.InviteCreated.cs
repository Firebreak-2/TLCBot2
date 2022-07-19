using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
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
    public static async Task InviteCreated() =>
        Program.Client.InviteCreated += async invite =>
        {
            const string name = nameof(DiscordSocketClient.InviteCreated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var fields = new Dictionary<string, object>
            {
                {"Invite Code", invite.Code},
                {"Inviter", invite.Inviter.Id},
                {"Seconds Until Expire", invite.MaxAge},
                {"Max Uses", invite.MaxUses},
            };

            await using (var db = new TlcDbContext())
            {
                await db.ServerInvites.AddAsync(new ServerInviteEntry(invite.Code));
                await db.SaveChangesAsync();
            }

            var logEntry = new LogEntry(name, importance, 
                $"[{invite.Inviter.Username}] created an invite [discord.gg/{invite.Code}]",
                tags,
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Invite Created")
                .AddField("Invite Link", $"https://discord.gg/{invite.Code}")
                .AddField("Inviter", invite.Inviter.Mention)
                .AddField("Expires", (DateTimeOffset.Now + invite.MaxAge.Seconds()).ToDynamicTimestamp())
                .AddField("Max Uses", invite.MaxUses)
            );
        };
}