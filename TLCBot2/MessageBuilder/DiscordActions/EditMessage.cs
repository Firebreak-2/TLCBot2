using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void EditMessage(IUserMessage message, MessageData newMessage)
    {
        Task.Run(async () => await message.ModifyAsync(props =>
        {
            if (newMessage.Embed is { })
                props.Embed = newMessage.Embed.ToEmbed();
            if (newMessage.Text is { })
                props.Content = newMessage.Text;
            if (newMessage.ComponentRows is { })
                props.Components = MessageData.MessageComponentsData.RowsToMessageComponent(newMessage.ComponentRows);
        }));
    }
    
    [DiscordAction]
    public static void EditMessageSimple(IUserMessage message, string newContent)
    {
        EditMessage(message, new MessageData{ Text = newContent });
    }
}