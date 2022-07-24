using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void SendMessage(ulong channel, MessageData messageData)
    {
        Task.Run(async () => await messageData.SendAsync(channel));
    }
    
    [DiscordAction]
    public static void SendMessageSimple(ulong channel, string text)
    {
        Task.Run(async () => await new MessageData{Text = text}.SendAsync(channel));
    }
}