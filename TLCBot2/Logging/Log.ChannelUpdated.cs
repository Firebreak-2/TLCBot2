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
    public static async Task ChannelUpdated() =>
        Program.Client.ChannelUpdated += async (oldChannel, newChannel) =>
        {
            if (newChannel is not SocketGuildChannel)
                return;

            const string name = nameof(DiscordSocketClient.ChannelUpdated);
            var auditLogs = await RuntimeConfig.FocusServer!.GetAuditLogsAsync(1, actionType: ActionType.ChannelUpdated).ToArrayAsync();

            var user = auditLogs[0].First().User;

            (Importance importance, string[] tags) = LoggingEventData.All[name];
            
            var oldDetails = oldChannel.GetChannelDetails();
            var newDetails = newChannel.GetChannelDetails();

            if (newDetails.ToJson() == oldDetails.ToJson())
                return;

            var logEntry = new LogEntry(name, importance, 
                $"{user.Username} updated [{oldChannel.GetChannelType()}] channel [{oldChannel.Id}]", 
                tags.Select(x => x.MappedFormat(
                        ("channel", oldChannel.Id),
                        ("user", user.Id))),
                new Dictionary<string, object>(2)
                {
                    {"OldChannel", oldDetails},
                    {"NewChannel", newDetails}
                }.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed =>
            {
                embed.WithTitle("Channel Updated");

                embed.AddField("Previous Details", oldChannel.GetChannelDetails(
                    "Category",
                    "Created",
                    "Is NSFW").ToJson().ToCodeBlock("json"));
                embed.AddField("Current Details", newChannel.GetChannelDetails(
                    "Category",
                    "Created",
                    "Is NSFW").ToJson().ToCodeBlock("json"));
                
                return embed
                    .AddField("Updated By", user.Mention)
                    .WithAuthor(user);
            });
        };
}