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
    public static async Task UserVoiceStateUpdated() =>
        Program.Client.UserVoiceStateUpdated += async (user, oldState, newState) =>
        {
            const string name = nameof(DiscordSocketClient.UserVoiceStateUpdated);
            (Importance importance, string[] tags) = LoggingEventData.All[name];

            var oldFields = GetDetails(oldState);
            var newFields = GetDetails(newState);

            var fields = oldFields.Keys
                .Where(x => oldFields[x] ^ newFields[x])
                .ToDictionary(
                    x => x,
                    x => new Dictionary<string, bool>(2)
                    {
                        {"Previous", oldFields[x]},
                        {"Current", newFields[x]},
                    });

            if (!fields.Any())
                return;

            var logEntry = new LogEntry(name, importance,
                $"[{user.Username}]'s voice state updated",
                tags.Select(x => x.MappedFormat(
                    ("user", user.Id)
                )),
                fields.ToJson());
            
            await ToFile(logEntry);

            await ToChannel(logEntry, embed => embed
                .WithTitle("User Voice State Updated")
                .WithFields(fields.Select(x => new EmbedFieldBuilder()
                    .WithName(x.Key)
                    .WithValue(x.Value.ToJson().ToCodeBlock("json"))))
            );
            
            Dictionary<string, bool> GetDetails(SocketVoiceState state) => 
                new()
                {
                    {"Server Deafened", state.IsDeafened},
                    {"Server Muted", state.IsMuted},
                    {"Self Deafened", state.IsSelfDeafened},
                    {"Self Muted", state.IsSelfMuted},
                    {"Streaming", state.IsStreaming},
                    {"Videoing", state.IsVideoing},
                };
        };
}