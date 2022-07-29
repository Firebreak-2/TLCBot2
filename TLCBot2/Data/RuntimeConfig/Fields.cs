using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Types;

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

    [RuntimeConfigField(ShortName = "cookiehelper")] 
    public static SocketRole? CanGiveCookiesRole;

    [RuntimeConfigField]
    public static SocketRole? CanAcceptSuggestionsRole;
    
    [RuntimeConfigField(ShortName = "backend")]
    public static SocketGuild? BackendServer;

    [RuntimeConfigField]
    public static long MotdCycleEpochTime;

    [RuntimeConfigField]
    public static SocketRole? MotdRole;

    [RuntimeConfigField]
    public static SocketRole? CantParticipateInMotdRole;

    [RuntimeConfigField]
    public static SocketRole? FrequentInviterAwardRole;

    [RuntimeConfigField]
    public static int InvitesRequiredForAward = 5;

    [RuntimeConfigField]
    public static int InvitesCookieAmountAward = 3;

    [RuntimeConfigField]
    public static int StarboardReactionsRequired = 5;

    [RuntimeConfigField(Json = true)] 
    public static string[] BlacklistedRoleMenuTags = Array.Empty<string>();

    [RuntimeConfigField(Json = true)] 
    public static RoleDescriptionPair[] ModProfessionRoles = Array.Empty<RoleDescriptionPair>();
}