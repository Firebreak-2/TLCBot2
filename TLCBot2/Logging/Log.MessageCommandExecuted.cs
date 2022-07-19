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
    public static async Task MessageCommandExecuted() =>
        Program.Client.MessageCommandExecuted += async messageCommand =>
        {
            const string name = nameof(DiscordSocketClient.MessageCommandExecuted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var fields = new Dictionary<string, object>
            {
                {"Command Name", messageCommand.CommandName}
            };

            string jsonFields = fields.ToJson();
            
            var logEntry = new LogEntry(name, importance, 
                $"[{messageCommand.User.Username}] used a message command [{messageCommand.CommandName}]",
                tags.Select(x => x.MappedFormat(
                    ("user", messageCommand.User.Id),
                    ("channel", (messageCommand.ChannelId?.ToString()).EnsureString()),
                    ("message", messageCommand.Data.Message.Id)
                )), jsonFields);

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Message Command Executed")
                .AddField("Executed On", messageCommand.Data.Message.GetJumpHyperLink())
                .AddField("Executer", messageCommand.User.Mention));
        };
}