using Discord.WebSocket;
using Humanizer;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task SlashCommandExecuted() =>
        Program.Client.SlashCommandExecuted += async slashCommand =>
        {
            const string name = nameof(DiscordSocketClient.SlashCommandExecuted);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var fields = new Dictionary<string, object>
            {
                {"Command Name", slashCommand.CommandName},
                {"Passed Arguments", slashCommand.Data.Options
                    .ToDictionary(
                        x => x.Name,
                        x => new
                        {
                            Value = x.Value?.ToString().EnsureString(), 
                            Type = x.Type.Humanize().EnsureString()
                        })}
            };

            string jsonFields = fields.ToJson();
            
            var logEntry = new LogEntry(name, importance, 
                $"[{slashCommand.User.Username}] used a slash command [{slashCommand.CommandName}]",
                tags.Select(x => x.MappedFormat(
                    ("user", slashCommand.User.Id),
                    ("channel", (slashCommand.ChannelId?.ToString()).EnsureString())
                )), jsonFields);

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Slash Command Executed")
                .AddField("Provided Arguments", fields["Passed Arguments"].ToJson().ToCodeBlock("json"))
                .AddField("Executer", slashCommand.User.Mention));
        };
}