using Discord;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.Listeners;

public static class ServerJoinListener
{
    public static string JoinHistoryPath => $"{Program.FileAssetsPath}\\serverInvitedHistory.txt";

    public static void Initialize()
    {
        if (!File.Exists(JoinHistoryPath))
            File.WriteAllText(JoinHistoryPath, "");
    }

    public static Task OnMemberJoined(SocketGuildUser user)
    {
        
        return Task.CompletedTask;
    }
}