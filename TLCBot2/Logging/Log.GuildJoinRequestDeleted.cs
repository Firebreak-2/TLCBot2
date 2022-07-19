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
    public static async Task GuildJoinRequestDeleted() =>
        Program.Client.GuildJoinRequestDeleted += async (userCache, guild) =>
        {
            if (guild.Id != RuntimeConfig.FocusServer?.Id)
                return;
            
            const string name = nameof(DiscordSocketClient.GuildJoinRequestDeleted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var user = await userCache.GetOrDownloadAsync();
            string userId = (user?.Id.ToString()).EnsureString();
            string userName = (user?.Username).EnsureString();
            
            var fields = new Dictionary<string, object>
            {
                {"User", userId}
            };
            
            var logEntry = new LogEntry(name, importance, 
                $"[{userName}] left the server without agreeing to the member screening",
                tags.Select(x => x.MappedFormat(
                    ("user", userId)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("User Quit Member Screening")
                .WithDescription("user joined and left the server before " +
                                 "agreeing to the member screening page")
                .AddField("User", $"<@!{userId}>")
            );
        };
}