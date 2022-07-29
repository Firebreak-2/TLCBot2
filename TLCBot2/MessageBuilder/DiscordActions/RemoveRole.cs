using Discord.WebSocket;
using TLCBot2.Attributes;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static void RemoveRole(SocketGuildUser user, SocketRole role)
    {
        Task.Run(async () => await user.RemoveRoleAsync(role));
    }
}