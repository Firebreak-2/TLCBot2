using Discord;
using Discord.WebSocket;

namespace TLCBot2;

public class Program
{
    public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    public static DiscordSocketClient Client;
    public static async Task MainAsync()
    {
        Client = new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 120 });
        Client.Log += Log;
        Client.Ready += Initialize;

        #region the token
        const string token = "OTQ1NjY0MjA0MDEzMjA3NjMy.YhTcaw.DwkNK9ZeZbXLNH46ucvP9kodje4";
        #endregion

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();
            
        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }
    private static Task Initialize()
    {
            
        return Task.CompletedTask;
    }
}