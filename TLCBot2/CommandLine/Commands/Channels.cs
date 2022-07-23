using System.Text;
using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    // [TerminalCommand(Description = "Shows a text representation of all the server's channels")]
    public static async Task Channels()
    {
        await ChannelTerminal.PrintAsync(GenerateGuildTextRepresentation(RuntimeConfig.FocusServer!.Id));
        // await ChannelTerminal.Channel.SendFileAsync();
    }

    public static string GenerateGuildTextRepresentation(ulong guildId)
    {
        var guild = Program.Client.GetGuild(guildId);
        string white = Helper.Ansi.Foreground.White.Get();
        const string reset = Helper.Ansi.Reset;

        StringBuilder builder = new();

        var channels = guild.Channels.OrderBy(x => x is SocketTextChannel tc
                ? tc.Category?.Position ?? 0
                : x.Position)
            .ThenBy(x => x is not SocketVoiceChannel)
            .ThenBy(x => x.Position);

        builder.Append($"{white}{guild.Name}{reset}\n");
        
        foreach (var channel in channels)
        {
            switch (channel)
            {
                case SocketCategoryChannel c:
                {
                    builder.Append($"\nv {c.Name.ToUpper()}");
                    break;
                }
                case SocketTextChannel c:
                {
                    string tube = ((SocketCategoryChannel) c.Category).Channels
                        .OrderBy(x => x.Position)
                        .Last() == c 
                            ? "└─" 
                            : "├─";
                    string channelIcon = c is SocketVoiceChannel ? "<" : "#";
                    
                    builder.Append($"{tube} {channelIcon} {c.Name}");
                    break;
                }
            }

            builder.Append('\n');
        }
        
        return builder.ToString();
    }
}