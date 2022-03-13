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
    public static async Task MainAsync()
    {
        Client = new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 100, LogGatewayIntentWarnings = false });
        Client.Log += Log;
        Client.Ready += Initialize;
        Client.MessageReceived += TlcConsole.OnMessageRecieved;
        Client.SlashCommandExecuted += CommandHandler.OnCommand;
        Client.ButtonExecuted += MessageComponentHandler.OnButtonExecuted;
        Client.SelectMenuExecuted += MessageComponentHandler.OnSelectionMenuExecuted;
        Client.MessageCommandExecuted += ContextCommandHandler.OnMessageCommandExecuted;
        Client.UserCommandExecuted += ContextCommandHandler.OnUserCommandExecuted;
        Client.ModalSubmitted += ModalHandler.OnModalRecieved;

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
        var prevCol = Console.ForegroundColor;
        Console.Write($"[{DateTime.Now.ToLongTimeString()}] [{msg.Source}]  ");
        Console.ForegroundColor = msg.Severity switch
        {
            LogSeverity.Critical => ConsoleColor.Yellow,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Warning or LogSeverity.Debug => ConsoleColor.DarkYellow,
            LogSeverity.Info => ConsoleColor.Green,
            LogSeverity.Verbose => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        };
        Console.WriteLine(msg.Message);
        if (msg.Exception != null)
            Console.WriteLine(msg.Exception);
        Console.ForegroundColor = prevCol;
        return Task.CompletedTask;
    }
    private static async Task Initialize()
    {
        TlcAllCommands.Initialize();
        LocateFilePath();

        await CommandHandler.Initialize();
        
        CookieManager.Initialize();
        ModalHandler.Initialize();
        MessageComponentHandler.Initialize();
    }

    private static void LocateFilePath()
    {
        FileAssetsPath = Directory.GetCurrentDirectory();
        bool found = false;
        while (!found && FileAssetsPath.Contains('\\'))
        {
            foreach (string file in Directory.GetDirectories(FileAssetsPath))
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