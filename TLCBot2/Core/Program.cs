using Discord;
using Discord.Interactions;
using Discord.Interactions.Builders;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core.CommandLine;
using TLCBot2.DataManagement;
using TLCBot2.Listeners;
using TLCBot2.Utilities;

namespace TLCBot2.Core;

public class Program
{
    public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    public static DiscordSocketClient Client = null!;
    public static string FileAssetsPath = "";
    public static async Task MainAsync()
    {
        var config = new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            LogGatewayIntentWarnings = true,
            GatewayIntents = GatewayIntents.All
        };
        Client = new DiscordSocketClient(config);
        Client.Log += Log;
        Client.Ready += Initialize;
        Client.MessageReceived += TlcConsole.OnMessageRecieved;
        Client.MessageReceived += DoodleOnlyListener.OnMessageRecieved;
        Client.MessageReceived += AutoThreadListener.OnMessageRecieved;
        Client.SlashCommandExecuted += SlashCommandHandler.OnCommand;
        Client.MessageCommandExecuted += ContextCommandHandler.OnMessageCommandExecuted;
        Client.UserCommandExecuted += ContextCommandHandler.OnUserCommandExecuted;
        Client.ButtonExecuted += MessageComponentHandler.OnButtonExecuted;
        Client.SelectMenuExecuted += MessageComponentHandler.OnSelectionMenuExecuted;
        Client.ModalSubmitted += ModalHandler.OnModalSubmitted;
        Client.ReactionAdded += StarboardListener.OnReactionAdded;
        Client.UserJoined += ServerJoinListener.OnMemberJoined;
        Client.InviteCreated += ServerJoinListener.OnInviteCreated;
        Client.UserJoined += ServerStatsListener.OnMemberJoined;
        Client.UserLeft += ServerStatsListener.OnMemberLeft;

        #region Token retrieval
        const string path = "token.txt";
        const string fileAssetsPath = "files_path.txt";
        string token;
        if (File.Exists(path))
        {
            token = (await File.ReadAllTextAsync(path)).Replace("\n", "");
            FileAssetsPath = (await File.ReadAllTextAsync(fileAssetsPath)).Replace("\n", "");
        }
        else if (File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/{path}"))
        {
            token = (await File.ReadAllTextAsync(
                $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/{path}")).Replace("\n", "");
            FileAssetsPath =
                (await File.ReadAllTextAsync(
                    $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/{fileAssetsPath}")).Replace("\n", "");
        }
        else
        {
            Console.WriteLine($"No token has been found. Shutting down...\n" +
                              $"Current Directory: {Directory.GetCurrentDirectory()}");
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
        Console.Write($"[{DateTime.Now.ToLongTimeString()}] [{msg.Source}]  ");
        var prevCol = Console.ForegroundColor;
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

    private static void PreInitialize()
    {
        // LocateFilePath();
        RuntimeConfig.Initialize();
        TlcAllCommands.Initialize();
    }
    private static async Task Initialize()
    {
        PreInitialize();
        // Helper.Sheets.Initialize();
        
        await ApplicationCommandManager.Initialize();
        StarboardListener.Initialize();
        ServerJoinListener.Initialize();
        CookieManager.Initialize();
        SocialMediaManager.Initialize();
        ServerStatsListener.Initialize();

        // Update += () =>
        // {
        //     for (int i = 0; i < MessageComponentHandler.AllComponents.Count; i++)
        //     {
        //         if (DateTime.Now <= MessageComponentHandler.AllComponents[i].BirthDate.AddMinutes(5)) continue;
        //         
        //         MessageComponentHandler.AllComponents.Remove(MessageComponentHandler.AllComponents[i]);
        //         break;
        //     }
        // };
    }
    private static void LocateFilePath()
    {
        FileAssetsPath = Directory.GetCurrentDirectory();
        bool found = false;
        while (!found && FileAssetsPath.Contains('/'))
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