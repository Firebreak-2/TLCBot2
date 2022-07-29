using System.Collections;
using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Utilities;

namespace TLCBot2.Core;

public static class Program
{
    public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    public static DiscordSocketClient Client;
    public static readonly string FileAssetsPath;
    public static bool DeveloperMode;

    static Program()
    {
        Client = new DiscordSocketClient(new DiscordSocketConfig
        {
            MessageCacheSize = 12000,
            LogGatewayIntentWarnings = true,
            GatewayIntents = GatewayIntents.All,
        });
        
        DeveloperMode = (Environment.GetEnvironmentVariable("DEV_MODE") ?? "").ToLower() == "true";

        const string filesPathEnvVariableName = "FILES_PATH";
        if (Environment.GetEnvironmentVariable(filesPathEnvVariableName) is not { } filesPath)
        {
            filesPath = null;
            Console.WriteLine($"No path for files has been found. Shutting down...\n" +
                              $"Enviroment variable name: {filesPathEnvVariableName}");
            Helper.Kill();
        }
        FileAssetsPath = filesPath!;
    }

    public static async Task MainAsync()
    {
        const string tokenEnvVariableName = "TOKEN";
        
        if (Environment.GetEnvironmentVariable(tokenEnvVariableName) is not { } token)
        {
            Console.WriteLine($"No token has been found. Shutting down...\n" +
                              $"Enviroment variable name: {tokenEnvVariableName}\n");
            return;
        }
        
        // Executes every method using the PreInitialize attribute
        foreach ((Func<Task> action, _) in PreInitializeAttribute.MethodsUsing.OrderByDescending(x => x.Attribute.Priority))
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
        foreach ((Func<Task> method, _) in InitializeAttribute.MethodsUsing.OrderByDescending(x => x.Attribute.Priority))
        {
            await Task.Run(method);
        }
    }
}