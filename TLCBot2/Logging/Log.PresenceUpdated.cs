using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Logging;

public static partial class Log
{ 
    [PreInitialize]
    public static async Task PresenceUpdated() =>
        Program.Client.PresenceUpdated += async (user, oldPresence, newPresence) =>
        {
            const string name = nameof(DiscordSocketClient.PresenceUpdated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var oldFields = new Dictionary<string, object>
            {
                {"Status", oldPresence.Status.ToString()},
                {"Active Clients", oldPresence.ActiveClients?.Select(x => x.ToString()).ToArray() ?? Array.Empty<string>()},
            };
            var newFields = new Dictionary<string, object>
            {
                {"Status", newPresence.Status.ToString()},
                {"Active Clients", newPresence.ActiveClients?.Select(x => x.ToString()).ToArray() ?? Array.Empty<string>()},
            };
            
            var fields = oldFields.Keys
                .Where(x => oldFields[x].ToJson() != newFields[x].ToJson())
                .ToDictionary(x => x, x => new Dictionary<string, object>(2)
                {
                    {"Previous", oldFields[x]},
                    {"Current", newFields[x]},
                });

            if (!fields.Any())
                return;
            
            var logEntry = new LogEntry(name, importance, 
                $"[{user.Username}]'s presence was updated",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id)
                )),
                fields.ToJson());

            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("Presence Updated")
                .WithFields(fields.Select(x => new EmbedFieldBuilder()
                    .WithName(x.Key)
                    .WithValue(x.Value.ToJson().ToCodeBlock("json"))))
                .AddField("User", user.Mention));
        };
}