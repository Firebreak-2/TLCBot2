using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task ThreadMemberJoined() =>
        Program.Client.ThreadMemberJoined += async user =>
        {
            const string name = nameof(DiscordSocketClient.ThreadMemberJoined);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}] joined a thread [{user.Thread.Name}]",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id),
                    ("thread", user.Thread.Id)
                )),
                "{}");

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("User Joiend Thread")
                .AddField("Thread", user.Thread.Mention)
                .AddField("User", user.Mention));
        };
}