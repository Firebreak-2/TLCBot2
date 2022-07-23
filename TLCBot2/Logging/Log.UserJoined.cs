using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Data.StringPrompts;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task UserJoined() =>
        Program.Client.UserJoined += async (user) =>
        {
            if (user.Guild.Id != RuntimeConfig.FocusServer?.Id)
                return;
            
            await using var db = new TlcDbContext();
            var oldInvites = db.ServerInvites.ToArray();
            
            bool canGiveCookies = db.Logs
                .Where(x => x.EventName == "UserBalanceUpdated")
                .ToArray()
                .All(x => x.Tags.FromJson<string[]>()!.First(y => y.StartsWith("USER="))
                    .Split('=')[1] != user.Id.ToString() 
                          && ((string) x.Data.FromJson<Dictionary<string, object>>()!["Reason"])
                          .StartsWith("Invited user "));

            const string name = nameof(DiscordSocketClient.UserJoined);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            IUser? inviter = null;

            if (canGiveCookies)
            {
                foreach (RestInviteMetadata invite in await user.Guild.GetInvitesAsync())
                {
                    if (oldInvites.TryFirst(x => x.InviteId == invite.Id, out var entry)
                        && invite.Uses <= entry!.Uses)
                        continue;
                    
                    inviter = invite.Inviter;

                    if (inviter.IsBot)
                        break;

                    int oldBalance = 0;

                    // give user cookies
                    if (await db.Users.FindAsync(user.Id) is { } cookieUser)
                    {
                        oldBalance = cookieUser.Balance;
                        cookieUser.Balance += RuntimeConfig.InvitesCookieAmountAward;
                    }
                    else
                    {
                        await db.Users.AddAsync(new ProfileEntry(user.Id)
                        {
                            Balance = RuntimeConfig.InvitesCookieAmountAward
                        });
                    }

                    // log it
                    await Log.CookieTransaction(null, user, oldBalance,
                        oldBalance + RuntimeConfig.InvitesCookieAmountAward,
                        $"Invited user [{user.Mention}]");

                    // send message in gen chat 🍪
                    await RuntimeConfig.FocusServer.SystemChannel.SendMessageAsync(
                        StringPrompts.InviteCookieAwardMessage.MappedFormat(
                            ("inviter", invite.Inviter.Mention),
                            ("invited", user.Mention)),
                        allowedMentions: AllowedMentions.None);
                    break;
                }
            }

            // fill fields with relevant data
            var fields = new Dictionary<string, object>
            {
                {"User", user.Id},
                {"Created At", user.CreatedAt.ToUnixTimeSeconds()},
                {"Had Nitro Since", (user.PremiumSince?.ToUnixTimeSeconds()).ToString().EnsureString()},
                {"Inviter", (inviter?.Id).ToString().EnsureString()}
            };
            
            // log the event
            
            var logEntry = new LogEntry(name, importance, 
                inviter is not null 
                    ? $"[{inviter.Username}] invited user [{user.Username}] to the server"
                    : $"[{user.Username}] joined the server",
                tags.Select(x => x.MappedFormat(
                    ("user", (inviter?.Id).ToString().EnsureString()),
                    ("targetUser", user.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("User Joined")
                .AddField("User", user.Mention)
                .AddField("Created At", user.CreatedAt.ToDynamicTimestamp())
                .AddField("Had Nitro Since", (user.PremiumSince?.ToDynamicTimestamp()).EnsureString())
                .AddField("Inviter", (inviter?.Mention).EnsureString()));
        }; 
}