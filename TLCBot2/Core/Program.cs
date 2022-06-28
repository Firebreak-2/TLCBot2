using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;

namespace TLCBot2.Core;

public class Program
{
    public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    public static DiscordSocketClient Client;
    public static string FileAssetsPath;
    public static bool DeveloperMode;

    static Program()
    {
        Client = new DiscordSocketClient(new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            LogGatewayIntentWarnings = true,
            GatewayIntents = GatewayIntents.All
        });
    }

    public static async Task MainAsync()
    {
        const string tokenEnvVariableName = "TOKEN";
        const string filesPathEnvVariableName = "FILES_PATH";

        if (Environment.GetEnvironmentVariable(filesPathEnvVariableName) is not { } filesPath)
        {
            Console.WriteLine($"No path for files has been found. Shutting down...\n" +
                              $"Enviroment variable name: {filesPathEnvVariableName}");
            return;
        }
        FileAssetsPath = filesPath;

        DeveloperMode = (Environment.GetEnvironmentVariable("DEV_MODE") ?? "").ToLower() == "true";


        if (Environment.GetEnvironmentVariable(tokenEnvVariableName) is not { } token)
        {
            Console.WriteLine($"No token has been found. Shutting down...\n" +
                              $"Enviroment variable name: {tokenEnvVariableName}\n");
            return;
        }
        
        // Executes every method using the PreInitialize attribute
        foreach (Func<Task> action in PreInitializeAttribute.MethodsUsing)
        {
            await Task.Run(action);
        }

        Client.Log += Log;
        Client.Ready += Initialize;

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private static Task Log(LogMessage msg)
    {
        return Task.Run(() =>
        {
            Console.Write($"[{DateTime.Now.ToLongTimeString()}] [{msg.Source}] ");
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
            Console.ResetColor();
        });
    }

    private static async Task Initialize()
    {
        // Executes every method using the Initialize attribute
        foreach (var method in InitializeAttribute.MethodsUsing)
        {
            await Task.Run(method);
        }
    }
}