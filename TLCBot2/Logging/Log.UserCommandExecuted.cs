using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task UserCommandExecuted() =>
        Program.Client.UserCommandExecuted += async userCommand =>
        {
            const string name = nameof(DiscordSocketClient.UserCommandExecuted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var fields = new Dictionary<string, object>
            {
                {"Command Name", userCommand.CommandName}
            };

            string jsonFields = fields.ToJson();
            
            var logEntry = new LogEntry(name, importance, 
                $"[{userCommand.User.Username}] used a user command [{userCommand.CommandName}]",
                tags.Select(x => x.MappedFormat(
                    ("user", userCommand.User.Id),
                    ("targetUser", userCommand.Data.Member.Id),
                    ("channel", (userCommand.ChannelId?.ToString()).EnsureString())
                )), jsonFields);

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("User Command Executed")
                .AddField("Executed On", userCommand.User.Mention)
                .AddField("Executer", "???"));
        };
}