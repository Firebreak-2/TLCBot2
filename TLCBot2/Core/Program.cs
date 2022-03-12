using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents;
using TLCBot2.Commands;
using TLCBot2.Cookies;
using TLCBot2.Core.CommandLine;
using TLCBot2.Utilities;

namespace TLCBot2.Core;

public class Program
{
    public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    public static DiscordSocketClient Client = null!;
    public static string FileAssetsPath = "";
    public static List<ApplicationCommandProperties> InitApplicationCommandProperties = new();
    public static async Task MainAsync()
    {
        Client = new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 120 });
        Client.Log += Log;
        Client.Ready += Initialize;
        Client.MessageReceived += TlcConsole.OnMessageRecieved;

        #region Token retrieval
        const string path = "token.txt";
        string token;
        if (File.Exists(path))
        {
            token = await File.ReadAllTextAsync(path);
        }
        else if (File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{path}"))
        {
            token = await File.ReadAllTextAsync($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{path}");
        }
        else
        {
            Console.WriteLine("No token has been found. Shutting down...");
            return;
        }
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
    private static async Task Initialize()
    {
        TlcAllCommands.Initialize();
        LocateFilePath();

        await TestCommands.Initialize();
        await CommercialCommands.Initialize();
        await Constants.Guilds.Lares!.BulkOverwriteApplicationCommandAsync(InitApplicationCommandProperties.ToArray());
        
        CookieManager.Initialize();
        ModalHandler.Initialize();
        MessageComponentHandler.Initialize();
    }

    private static void LocateFilePath()
    {
        FileAssetsPath = Directory.GetCurrentDirectory();
        var found = false;
        while (!found && FileAssetsPath.Contains('\\'))
        {
            foreach (var file in Directory.GetDirectories(FileAssetsPath))
            {
                if (!file.EndsWith("TLC_Files")) continue;
                FileAssetsPath = file;
                found = true;
                break;
            }

            if (!found)
                FileAssetsPath = Helper.GoBackDirectory(FileAssetsPath);
        }
    }
}