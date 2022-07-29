using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void SendPrivateMessage(SocketUser user, MessageData message)
    {
        Task.Run(async () =>
        {
            await user.SendMessageAsync(
                text: message.Text ?? "",
                embed: message.Embed?.ToEmbed(),
                components: MessageData.MessageComponentsData.RowsToMessageComponent(message.ComponentRows));
        });
    }
    
    [DiscordAction]
    public static void SendPrivateMessageSimple(SocketUser user, string text)
    {
        SendPrivateMessage(user, new MessageData{ Text = text});
    }
}