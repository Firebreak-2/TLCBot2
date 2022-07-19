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
    public static async Task ChannelCreated() =>
        Program.Client.ChannelCreated += async channel =>
        {
            if (channel is not SocketGuildChannel)
                return;

            const string name = nameof(DiscordSocketClient.ChannelCreated);
            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.ChannelCreated).ToArrayAsync();

            var user = auditLogs[0].First().User;

            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var logEntry = new LogEntry(name, importance, 
                $"{user.Username} created [{channel.GetChannelType()}] channel [{channel.Id}]", 
                tags.Select(x => x.MappedFormat(
                        ("channel", channel.Id),
                        ("user", user.Id))),
                channel.GetChannelDetails().ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Channel Created")
                .AddField("Channel Details", channel.GetChannelDetails(
                    "Category",
                    "Created",
                    "Is NSFW").ToJson().ToCodeBlock("json"))
                .AddField("Created By", user.Mention)
                .WithAuthor(user));
        };
}