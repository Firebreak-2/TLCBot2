using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void CreateThread(SocketTextChannel channel, 
        string name, 
        bool isPublic,
        ThreadArchiveDuration autoArchiveDuration,
        IMessage? attachedMessage)
    {
        Task.Run(async () =>
        {
            await channel.CreateThreadAsync(name,
                isPublic
                    ? ThreadType.PublicThread
                    : ThreadType.PrivateThread,
                autoArchiveDuration,
                attachedMessage);
        });
    }
}