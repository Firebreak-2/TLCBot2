using System.Diagnostics;
using System.Text.RegularExpressions;
using Discord;
using TLCBot2.Cookies;
using TLCBot2.Utilities;

namespace TLCBot2.Core.CommandLine;

public static class TlcAllCommands
{
    public static void Initialize()
    {
        AddCommand(new TlcCommand("reinitcmds", _ =>
        {
            TlcConsole.ListCommand.Clear();
            Initialize();
            TlcConsole.Print("reinitialization complete");
        }, description:"Reinitializes all commands for real-time debugging"));
        
        AddCommand(new TlcCommand("help", args =>
        {
            if (args.Length == 0)
                TlcConsole.Print(string.Join("\n", TlcConsole.ListCommand.Where(x => x.Name != "help")
                    .Select(x => $"{x.Name}:\n  Args: {x.Args}\n  Description: {x.Description}\n")));
            else
            {
                var output = 
                   (from item in args 
                    where TlcConsole.ListCommand
                        .Any(x => 
                            string.Equals(x.Name, item, StringComparison.CurrentCultureIgnoreCase))
                    select TlcConsole.ListCommand
                        .First(x =>
                            string.Equals(x.Name, item, StringComparison.CurrentCultureIgnoreCase)))
                    .Aggregate("", (current, cmd) =>
                        current + $"{cmd.Name}:\n  {cmd.Description}\n  Args: {cmd.Args}\n\n");

                TlcConsole.Print(output);
            }
        }, -1, "sends over a real life minion to help you out in tough situations"));
        
        AddCommand(new TlcCommand("cookies", args =>
        {
            switch (args[0])
            {
                case "add":
                case "set":
                case "edit":
                    CookieManager.AddOrEditUserToDatabase(
                        userId:      ulong.Parse(args[1]),
                        cookieCount: int.Parse(args[2]),
                        isBanned:    bool.Parse(args[3]));
                    TlcConsole.Print($"added/edited {args[1]}'s entry");
                    break;
                case "remove":
                    TlcConsole.Print(CookieManager.RemoveUserFromDatabase(ulong.Parse(args[1]))
                        ? $"removed {args[1]} from the database"
                        : $"user {args[1]} does not exist");
                    break;
                case "give":
                    CookieManager.TakeOrGiveCookiesToUser(ulong.Parse(args[1]), int.Parse(args[2]));
                    TlcConsole.Print($"given {args[1]} {args[2]} cookies");
                    break;
                case "setall":
                    CookieManager.ResetDatabase(_ => int.Parse(args[1]), _ => bool.Parse(args[2]));
                    TlcConsole.Print("modified the data of all entires");
                    break;
                case "reset":
                    CookieManager.ResetDatabase();
                    TlcConsole.Print("reset the data of all entries");
                    break;
                case "removeall":
                    CookieManager.ClearDatabase();
                    TlcConsole.Print("removed all database entries");
                    break;
                case "get":
                    TlcConsole.Print(
                        CookieManager.GetUserFromDatabase(ulong.Parse(args[1]), out var cookies, out var isBanned)
                            ? $"user: {args[1]}\n  cookies: {cookies}\n  isBanned: {isBanned}"
                            : "user does not exist");
                    break;
                case "database":
                    TlcConsole.Print(File.ReadAllText($"{Program.FileAssetsPath}\\database.cookie"));
                    break;
                default:
                    TlcConsole.Print($"option not found: {args[0]}");
                    break;
            }
        }, -1, "cookie manipulation"));
        
        AddCommand(new TlcCommand("kill", _ =>
        {
            TlcConsole.Print("goodbye world");
            Program.Client.LogoutAsync();
            Process.GetCurrentProcess().Kill();
        }));
        
        AddCommand(new TlcCommand("ping", _ =>
        {
            TlcConsole.Print("!pong", "diff");
        }, description: "responds with \"!pong\""));
        
        AddCommand(new TlcCommand("post", args =>
        {
            Constants.Channels.Lares.TLCBetaCommandLine
                .SendFileAsync(args[0], null);
        }, 1));
        
        AddCommand(new TlcCommand("ls", args =>
        {
            TlcConsole.Print(string.Join("\n\n",
                Directory.GetFiles(args[0]).Union(Directory.GetDirectories(args[0]))));
        }, 1));
        
        AddCommand(new TlcCommand("return", _ =>
        {
            TlcConsole.Print(Program.FileAssetsPath);
        }));
        
        AddCommand(new TlcCommand("calc", args =>
        {
            TlcConsole.Print($">>> {Helper.Compute(args[0])}");
        }, 1, "Parses the given input through a calculator and returns the output"));
        
        AddCommand(new TlcCommand("echo", args =>
        {
            TlcConsole.Print($"{args[0]}");
        }, 1, "Repeats the input"));
    }

    public static void AddCommand(TlcCommand command) => TlcConsole.ListCommand.Add(command);
}