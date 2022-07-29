using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction]
    public static DiscordActionResult<object> GetFromString(string @string, DiscordConversionType @as)
    {
        Type type = @as switch
        {
            DiscordConversionType.Message => typeof(SocketUserMessage),
            DiscordConversionType.Channel => typeof(SocketGuildChannel),
            DiscordConversionType.Guild => typeof(SocketGuild),
            DiscordConversionType.User => typeof(SocketUser),
            DiscordConversionType.Role => typeof(SocketRole),
            _ => throw new ArgumentOutOfRangeException(nameof(@as), @as, null)
        };

        return DiscordActionResult<object>.From(() =>
            Helper.ConvertFromString(@string, type) 
                ?? throw new Exception($"Cannot convert [{@string}] to type [{@as.ToString()}]"));
    }

    public enum DiscordConversionType
    {
        Message,
        Channel,
        Guild,
        User,
        Role,
    }
}