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
    public static DiscordSocketClient BetaClient = null!;
    public static DiscordSocketClient DeltaClient = null!;
    public static string FileAssetsPath = "";
    public static async Task MainAsync()
    {
        BetaClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            MessageCacheSize = 100, 
            LogGatewayIntentWarnings = true,
            GatewayIntents = GatewayIntents.All
        });
        BetaClient.Log += BetaLog;
        DeltaClient.Log += DeltaLog;
        BetaClient.Ready += Initialize;
        BetaClient.MessageReceived += TlcConsole.OnMessageRecieved;
        BetaClient.MessageReceived += DoodleOnlyListener.OnMessageRecieved;
        BetaClient.SlashCommandExecuted += SlashCommandHandler.OnCommand;
        BetaClient.MessageCommandExecuted += ContextCommandHandler.OnMessageCommandExecuted;
        BetaClient.UserCommandExecuted += ContextCommandHandler.OnUserCommandExecuted;
        BetaClient.ButtonExecuted += MessageComponentHandler.OnButtonExecuted;
        BetaClient.SelectMenuExecuted += MessageComponentHandler.OnSelectionMenuExecuted;
        BetaClient.ModalSubmitted += ModalHandler.OnModalSubmitted;
        BetaClient.ReactionAdded += StarboardListener.OnReactionAdded;
        BetaClient.UserJoined += ServerJoinListener.OnMemberJoined;
        BetaClient.UserJoined += ServerStatsListener.OnMemberJoined;
        BetaClient.UserLeft += ServerStatsListener.OnMemberLeft;
        BetaClient.InviteCreated += ServerJoinListener.OnInviteCreated;

        #region Token retrieval
        const string path = "token.txt";
        const string path2 = "token2.txt";
        string token;
        string token2;
        if (File.Exists(path))
        {
            token = await File.ReadAllTextAsync(path);
            token2 = await File.ReadAllTextAsync(path2);
        }
        else if (File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{path}"))
        {
            token = await File.ReadAllTextAsync($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{path}");
            token2 = await File.ReadAllTextAsync($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{path2}");
        }
        else
        {
            Console.WriteLine("No token has been found. Shutting down...");
            return;
        }
        #endregion
        
        await BetaClient.LoginAsync(TokenType.Bot, token);
        await BetaClient.StartAsync();
        await DeltaClient.LoginAsync(TokenType.Bot, token2);
        await DeltaClient.StartAsync();
            
        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
    private static Task BetaLog(LogMessage msg) => Log(msg, "Beta");
    private static Task DeltaLog(LogMessage msg) => Log(msg, "Delta");
    private static Task Log(LogMessage msg, string botName)
    {
        Console.Write($"[{botName}] [{DateTime.Now.ToLongTimeString()}] [{msg.Source}]  ");
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
        Console.ForegroundColor = ConsoleColor.White;
        return Task.CompletedTask;
    }
    private static async Task Initialize()
    {
        LocateFilePath();
        TlcAllCommands.Initialize();
        RuntimeConfig.Initialize();

        await ApplicationCommandHandler.Initialize();
        StarboardListener.Initialize();
        ServerJoinListener.Initialize();
        CookieManager.Initialize();
        SocialMediaManager.Initialize();
        ServerStatsListener.Initialize();

        // Update += () =>
        // {
        //     for (int i = 0; i < MessageComponentHandler.AllComponents.Count; i++)
        //     {
        //         if (DateTime.Now <= MessageComponentHandler.AllComponents[i].BirthDate.AddSeconds(5)) continue;
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