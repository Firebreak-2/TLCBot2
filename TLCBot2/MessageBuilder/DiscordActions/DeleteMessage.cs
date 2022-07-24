using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void DeleteMessage(ulong channel, ulong messageId)
    {
        SocketTextChannel textChannel =
            (SocketTextChannel) Program.Client.GetChannel(channel);
        
        Task.Run(async () => await textChannel.DeleteMessageAsync(messageId));
    }
}