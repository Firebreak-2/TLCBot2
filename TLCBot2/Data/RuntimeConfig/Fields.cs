using Discord.WebSocket;
using TLCBot2.Attributes;

namespace TLCBot2.Data.RuntimeConfig;

public static partial class RuntimeConfig
{
    [RuntimeConfigField(ShortName = "dump")]
    public static SocketTextChannel? FileDumpChannel;
    
    [RuntimeConfigField(ShortName = "json")]
    public static SocketTextChannel? JsonDataChannel;

    [RuntimeConfigField(ShortName = "logs")]
    public static SocketTextChannel? ServerLogsChannel;

    [RuntimeConfigField(ShortName = "feedback")]
    public static SocketTextChannel? UserFeedbackChannel;

    [RuntimeConfigField(ShortName = "reports")]
    public static SocketTextChannel? BotReportsChannel;

    [RuntimeConfigField(ShortName = "suggestions")]
    public static SocketTextChannel? ServerSuggestionsChannel;

    [RuntimeConfigField]
    public static SocketTextChannel? QotdChannel;

    [RuntimeConfigField]
    public static SocketRole? QotdRole;

    [RuntimeConfigField] 
    public static SocketRole? CanGiveCookiesRole;

    [RuntimeConfigField]
    public static SocketRole? CanAcceptSuggestionsRole;
    
    [RuntimeConfigField(ShortName = "backend")]
    public static SocketGuild? BackendServer;
}