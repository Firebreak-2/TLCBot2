using Discord.WebSocket;
using TLC_Beta.Core;

namespace TLC_Beta.Utilities;

public static class Constants
{
    public static class Guilds
    {
        public static class Id
        {
            public const ulong TlcBotSupport = 725116205081428070;
            public const ulong Tlc = 616111158549544960;
            public const ulong Lares = 915280394117591130;
        }
        public static SocketGuild TlcBotSupport = Program.Client.GetGuild(Id.TlcBotSupport);
        public static SocketGuild Tlc = Program.Client.GetGuild(Id.Tlc);
        public static SocketGuild? Lares = Program.Client.GetGuild(Id.Lares);
    }
    public static class Users
    {
        public const ulong Firebreak = 751535897287327865;
        public static bool IsDev(ulong userId) => userId == Firebreak;
        public static bool IsDev(SocketUser user) => IsDev(user.Id);
    }

    public static class Channels
    {
        public static class Lares
        {
            public static class Id
            {
                public const ulong Coloore = 950723898591285268;
                public const ulong DefaultFileDump = 950771941264982083;
                public const ulong TLCBetaCommandLine = 951848832516358154;
            }
            public static SocketTextChannel Coloore = Guilds.Lares.GetTextChannel(Id.Coloore);
            public static SocketTextChannel DefaultFileDump = Guilds.Lares.GetTextChannel(Id.DefaultFileDump);
            public static SocketTextChannel TLCBetaCommandLine = Guilds.Lares.GetTextChannel(Id.TLCBetaCommandLine);
        }
    }
}