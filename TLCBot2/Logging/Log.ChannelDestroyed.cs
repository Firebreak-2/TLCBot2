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
    public static async Task ChannelDestroyed() =>
        Program.Client.ChannelDestroyed += async channel =>
        {
            if (channel is not SocketGuildChannel)
                return;
            
            const string name = nameof(DiscordSocketClient.ChannelDestroyed);
            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.ChannelDeleted).ToArrayAsync();
            
            var user = auditLogs[0].First().User;
            
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var logEntry = new LogEntry(name, importance,
                $"{user.Username} deleted [{channel.GetChannelType()}] channel [{channel.Id}]", 
                tags.Select(x => x.MappedFormat(
                        ("channel", channel.Id),
                        ("user", user.Id))),
                channel.GetChannelDetails().ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Channel Deleted")
                .AddField("Channel Details", channel.GetChannelDetails(
                    "Category",
                    "Created",
                    "Is NSFW").ToJson().ToCodeBlock("json"))
                .AddField("Deleted By", user.Mention)
                .WithAuthor(user));
        };
}