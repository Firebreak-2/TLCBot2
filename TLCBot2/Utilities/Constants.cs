using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.Utilities;

public static class Constants
{
    public static class Guilds
    {
        public static class Id
        {
            public const ulong TlcBotSupport = 725116205081428070;
            public const ulong TlcBetaTesting = 954086398787469382;
            public const ulong Tlc = 616111158549544960;
            public const ulong Lares = 915280394117591130;
        }
        public static SocketGuild TlcBotSupport = Program.Client.GetGuild(Id.TlcBotSupport);
        public static SocketGuild TlcBetaTesting = Program.Client.GetGuild(Id.TlcBetaTesting);
        public static SocketGuild Tlc = Program.Client.GetGuild(Id.Tlc);
        public static SocketGuild? Lares = Program.Client.GetGuild(Id.Lares);
    }
    public static class Users
    {
        public const ulong Firebreak = 751535897287327865;
        public static bool IsDev(SocketUser user) => 
            user.MutualGuilds.Any(x => x.Id == RuntimeConfig.AdminRole.Guild.Id) 
            && RuntimeConfig.AdminRole.Guild.GetUser(user.Id).Roles
                .Any(x => x.Id == RuntimeConfig.AdminRole.Id);
    }

    public static class Channels
    {
        public static class Lares
        {
            public static class Id
            {
                public const ulong TLCBetaCommandLine = 951848832516358154;
            }
        }
    }
}