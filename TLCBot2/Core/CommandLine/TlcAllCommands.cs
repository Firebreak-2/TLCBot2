using System.Diagnostics;
using System.Text.RegularExpressions;
using Discord;
using SixLabors.Fonts;
using TLCBot2.Cookies;
using TLCBot2.Utilities;
using Color = Discord.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

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
        
        AddCommand(new TlcCommand("test", _ =>
        {
            using var image = new Image<Argb32>(1000, 1000);
            int tilesPerRow = 5;
            var bingoPrompts = 
                File.ReadAllLines($"{Program.FileAssetsPath}\\bingoPrompts.cfg")
                    .Select(x => x.Replace(' ', '\n')).ToHashSet();
            
            const int gridLineWidth = 5;
            int tileWidth = image.Width / tilesPerRow;
            int tileHeight = image.Height / tilesPerRow;
            
            image.FillColor((x, y) =>
            {
                for (int i = 0; i < gridLineWidth; i++)
                {
                    for (int j = 0; j < tilesPerRow; j++)
                    {
                        int val = i + image.Width / tilesPerRow * j;
                        if (val <= gridLineWidth) continue;
                
                        if (x == val || y == val) return new Argb32(0, 0, 0);
                    }
                }
                return new Argb32(255, 255, 255);
            });

            var centerPos = Point.Empty;
            for (int y = 0; y < tilesPerRow; y++)
            {
                for (int x = 0; x < tilesPerRow; x++)
                {
                    int yPos = y * image.Height / tilesPerRow;
                    int xPos = x * image.Width  / tilesPerRow;

                    if (x == y && x == tilesPerRow / 2)
                    {
                        centerPos = new Point(xPos, yPos);
                        continue;
                    }
    
                    string prompt = bingoPrompts.RandomChoice();
                    bingoPrompts.Remove(prompt);
                    var font = SystemFonts.CreateFont("Arial", prompt.Length > 8 ? 35 : 42);
                    var options = new TextOptions(font)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Origin = new PointF(xPos + tileWidth / 2, yPos + tileHeight / 2),
                        WordBreaking = WordBreaking.BreakAll,
                        WrappingLength = tileWidth
                    };
                    image.Mutate(img =>
                        img.DrawText(
                            options,
                            prompt,
                            SixLabors.ImageSharp.Color.Black
                            ));
                }
            }
    
            using var imgToBeDrawn = Image.Load<Argb32>($"{Program.FileAssetsPath}\\TLC_Logo.png");
            imgToBeDrawn.Mutate(img =>
                img.Resize(tileWidth, tileHeight));
            image.Mutate(img =>
                img.DrawImage(imgToBeDrawn, centerPos, 1));
    
            var embed = new EmbedBuilder()
                .WithTitle($"TLC bingo card for {"Firebreak"}")
                .WithDescription(
                    "Draw an image that would score a bingo on the following sheet. Don't forget to shout bingo and share your finished drawing!")
                .WithImageUrl(Helper.GetFileUrl(image.ToStream(), Constants.Channels.Lares.DefaultFileDump))
                .WithColor(Color.Blue);
            
            Constants.Channels.Lares.TLCBetaCommandLine.SendMessageAsync(embed: embed.Build());
        }));
    }

    public static void AddCommand(TlcCommand command) => TlcConsole.ListCommand.Add(command);
}