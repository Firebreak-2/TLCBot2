using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.Interactions.Builders;
using Discord.WebSocket;
using Humanizer;
using Newtonsoft.Json;
using TLCBot2.ApplicationComponents;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core.CommandLine;
using TLCBot2.DataManagement;
using TLCBot2.DataManagement.Cookies;
using TLCBot2.Listeners;
using TLCBot2.Listeners.TimedEvents;
using TLCBot2.Utilities;
using Timer = System.Timers.Timer;

namespace TLCBot2.Core;

public class Program
{
    public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    public static DiscordSocketClient Client = null!;
    public static string FileAssetsPath = "";
    public static async Task MainAsync()
    {
        // var values = new[]
        // {
        //     new[] { new[] { "Join Request Deleted" }, new[] { "Neutral", "join request deleted" } },
        //     new[] { new[] { "Thread Member Left" }, new[] { "Useless", "thread member left", "user change", "threads", "user={user.Id}" } },
        //     new[] { new[] { "Thread Member Joined" }, new[] { "Useless", "thread member joined", "user change", "threads", "user={user.Id}" } },
        //     new[] { new[] { "Event User Added" }, new[] { "Useless", "events", "server change", "event user added", "user={user.Id}" } },
        //     new[] { new[] { "Event User Removed" }, new[] { "Useless", "events", "server change", "event user removed", "user={user.Id}" } },
        //     new[] { new[] { "Server Event Updated" }, new[] { "Neutral", "events", "server change", "server event updated" } },
        //     new[] { new[] { "Server Event Cancelled" }, new[] { "Neutral", "events", "server change", "server event cancelled" } },
        //     new[] { new[] { "Server Event Completed" }, new[] { "Neutral", "events", "server change", "server event completed" } },
        //     new[] { new[] { "Server Event Created" }, new[] { "Neutral", "events", "server change", "server event created" } },
        //     new[] { new[] { "Server Event Started" }, new[] { "Neutral", "events", "server change", "server event started" } },
        //     new[] { new[] { "User Voice State Updated" }, new[] { "Useless", "user voice state updated", "user change", "user={user.Id}" } },
        //     new[] { new[] { "User Updated" }, new[] { "Neutral", "user updated", "user change", "user={user.Id}" } },
        //     new[] { new[] { "User Joined" }, new[] { "Neutral", "user joined", "user change", "server change", "user={user.Id}" } },
        //     new[] { new[] { "User Left" }, new[] { "Neutral", "user left", "user change", "server change", "user={user.Id}" } },
        //     new[] { new[] { "User Banned" }, new[] { "Warning", "ban", "user banned", "user change", "server change", "user={user.Id}" } },
        //     new[] { new[] { "User Unbanned" }, new[] { "Warning", "ban", "user unbanned", "user change", "server change", "user={user.Id}" } },
        //     new[] { new[] { "Thread User Joined" }, new[] { "Useless", "thread user joined", "threads", "user={user.Id}" } },
        //     new[] { new[] { "Thread User Left" }, new[] { "Useless", "thread user left", "threads", "user={user.Id}" } },
        //     new[] { new[] { "Thread Created" }, new[] { "Neutral", "threads", "thread created", "server change" } },
        //     new[] { new[] { "Thread Deleted" }, new[] { "Neutral", "threads", "thread deleted", "server change" } },
        //     new[] { new[] { "Thread Updated" }, new[] { "Neutral", "threads", "thread updated", "server change" } },
        //     new[] { new[] { "Stage Updated" }, new[] { "Useless", "stage", "stage updated", "server change" } },
        //     new[] { new[] { "Stage Started" }, new[] { "Neutral", "stage", "stage started", "server change" } },
        //     new[] { new[] { "Stage Ended" }, new[] { "Neutral", "stage", "stage ended", "server change" } },
        //     new[] { new[] { "Speaker Added" }, new[] { "Useless", "stage", "user change", "speaker added", "user={user.Id}" } },
        //     new[] { new[] { "Speaker Removed" }, new[] { "Useless", "stage", "user change", "speaker removed", "user={user.Id}" } },
        //     new[] { new[] { "Role Created" }, new[] { "Warning", "roles", "role created", "server change" } },
        //     new[] { new[] { "Role Deleted" }, new[] { "Dangerous", "roles", "role deleted", "server change" } },
        //     new[] { new[] { "Role Updated" }, new[] { "Warning", "roles", "role updated", "server change" } },
        //     new[] { new[] { "Reactions Cleared For Message" }, new[] { "Neutral", "reactions", "reaction removed", "reactions cleared", "reactions cleared for message", "message change" } },
        //     new[] { new[] { "Reactions Cleared For Emote" }, new[] { "Neutral", "reactions", "reaction removed", "reactions cleared", "reactions cleared for emote", "message change" } },
        //     new[] { new[] { "Reaction Removed" }, new[] { "Useless", "reactions", "reaction removed", "message change", "user={user.Id}" } },
        //     new[] { new[] { "Reaction Added" }, new[] { "Useless", "reactions", "reaction added", "message change", "user={user.Id}" } },
        //     new[] { new[] { "Status Changed" }, new[] { "Useless", "status changed", "user change", "status", "user={user.Id}" } },
        //     new[] { new[] { "Message Edited" }, new[] { "Neutral", "message edited", "message change", "channel={channel.Id}", "user={user.Id}" } },
        //     new[] { new[] { "Bulk Messages Deleted" }, new[] { "Warning", "message deleted", "message change", "bulk messages deleted", "channel={channel.Id}" } },
        //     new[] { new[] { "Message Deleted" }, new[] { "Warning", "message deleted", "message change", "user={user.Id}" } },
        //     new[] { new[] { "Invite Created" }, new[] { "Neutral", "invite created", "server change", "user={user.Id}" } },
        //     new[] { new[] { "Invite Deleted" }, new[] { "Neutral", "invite deleted", "server change", "channel={inviteChannel.Id}" } },
        //     new[] { new[] { "Sticker Created" }, new[] { "Warning", "sticker created", "server change" } },
        //     new[] { new[] { "Sticker Deleted" }, new[] { "Warning", "sticker deleted", "server change" } },
        //     new[] { new[] { "Sticker Updated" }, new[] { "Neutral", "sticker updated", "server change" } },
        //     new[] { new[] { "Channel Updated" }, new[] { "Warning", "channel updated", "server change", "channel={newChannel.Id}" } },
        //     new[] { new[] { "Channel Created" }, new[] { "Warning", "channel created", "server change", "channel={channel.Id}" } },
        //     new[] { new[] { "Channel Deleted" }, new[] { "Dangerous", "channel deleted", "server change", "channel={channel.Id}" } },
        //     new[] { new[] { "Slash Command Executed" }, new[] { "Useless", "slash command executed", "command", "interaction", "user={user.Id}" } },
        //     new[] { new[] { "Modal Submitted" }, new[] { "Useless", "modal submitted", "interaction", "user={user.Id}" } },
        //     new[] { new[] { "Selection Menu Executed" }, new[] { "Useless", "selection menu executed", "message component", "interaction", "user={user.Id}" } },
        //     new[] { new[] { "Button Executed" }, new[] { "Useless", "button executed", "message component", "interaction", "user={user.Id}" } },
        //     new[] { new[] { "User Command Executed" }, new[] { "Useless", "user command executed", "command", "interaction", "user={user.Id}" } },
        //     new[] { new[] { "Message Command Executed" }, new[] { "Useless", "message command executed", "command", "interaction", "user={user.Id}" } },
        // };
        // string tagify(string y) => "TAG_" + y.ToUpper().Replace(' ', '_');
        // Console.WriteLine(JsonConvert.SerializeObject(values.Select(x => new[] {x[0], x[1].Select(tagify)}), Formatting.Indented));
        // return;
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
        Client.MessageReceived += CritiqueOnlyListener.OnMessageRecieved;
        Client.MessageReceived += AutoThreadListener.OnMessageRecieved;
        Client.MessageReceived += RepostListener.OnMessageReceived;
        Client.MessageReceived += VentingTagListener.OnMessageReceived;
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
        TLCLogListener.PreInitialize();
        AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
        {
            await Client.LogoutAsync();
        };

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
        TheFlowOfTime.Initialize();
    }
    private static async Task Initialize()
    {
        PreInitialize();
        
        await ApplicationCommandManager.Initialize();
        StarboardListener.Initialize();
        ServerJoinListener.Initialize();
        CookieManager.Initialize();
        SocialMediaManager.Initialize();
        ServerStatsListener.Initialize();
        BotMessageReminders.Initialize();
        UserRoleBans.Initialize();
        ChannelVisiblityData.Initialize();

        try
        {
            await RuntimeConfig.TLCBetaCommandLine.SendMessageAsync(
                $"Bot Initialized at <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>");
        }
        catch
        {
            Console.WriteLine("Initialization message failed. Consider looking at RuntimeConfig.txt");
        }
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
