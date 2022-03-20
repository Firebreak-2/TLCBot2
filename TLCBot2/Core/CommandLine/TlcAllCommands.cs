using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using MoreLinq;
using Newtonsoft.Json;
using SixLabors.Fonts;
using TLCBot2.Utilities;
using Color = Discord.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.ApplicationComponents.Eternal;
using TLCBot2.DataManagement;
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
                    CookieManager.AddOrModifyUser(
                        userID:   ulong.Parse(args[1]),
                        cookies:  int.Parse(args[2]),
                        isBanned: bool.Parse(args[3]));
                    TlcConsole.Print($"added/edited {args[1]}'s entry");
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
                        CookieManager.GetUser(ulong.Parse(args[1]), out var entry)
                            ? $"user: {args[1]}\n  cookies: {entry.Cookies}\n  isBanned: {entry.IsBanned}"
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
            Program.BetaClient.LogoutAsync();
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
        
        AddCommand(new TlcCommand("makeeternalmessage", args =>
        {
            var channel = (SocketTextChannel)Program.BetaClient.GetChannel(ulong.Parse(args[1]));
            switch (int.Parse(args[0]))
            {
                case 0:
                {
                    void Stuff(
                        string name,
                        Func<SocketTextChannel, IEnumerable<SocketRole>> roles,
                        Func<SocketTextChannel, SelectMenuBuilder> eternalMenu,
                        bool canSelectMultiple = false)
                    {
                        var categoricalRoles = roles(channel);
                        channel.SendMessageAsync($"**{name}**\n" + string.Join("\n",
                                categoricalRoles.Select(x => x.Mention)),
                            components: FireMessageComponent.CreateNew(
                                new FireMessageComponent(new ComponentBuilder()
                                        .WithSelectMenu(eternalMenu(channel)
                                            .AddOption("Remove All", "rmv")
                                            .WithMaxValues(canSelectMultiple ? categoricalRoles.Count() : 1)),
                                    null, null)));
                        Thread.Sleep(1000);
                    }

                    Stuff("Color Roles", EternalSelectMenus.GetColorRoles, EternalSelectMenus.EternalSelectMenu0);
                    Stuff("Pronoun Roles", EternalSelectMenus.GetPronounRoles, EternalSelectMenus.EternalSelectMenu1);
                    Stuff("Ping Roles", EternalSelectMenus.GetPingRoles, EternalSelectMenus.EternalSelectMenu2, true);
                    Stuff("Bot Fun Roles", EternalSelectMenus.GetBotFunRoles, EternalSelectMenus.EternalSelectMenu3,
                        true);
                    Stuff("Art Specialty Roles", EternalSelectMenus.GetArtSpecialityRoles,
                        EternalSelectMenus.EternalSelectMenu4, true);
                    Stuff("Misc Roles", EternalSelectMenus.GetMiscRoles, EternalSelectMenus.EternalSelectMenu5, true);
                }
                    break;
                case 1:
                {
                    void Stuff(string messageText, ButtonBuilder eternalButton)
                    {
                        channel.SendMessageAsync(messageText, 
                            components: new FireMessageComponent(new ComponentBuilder()
                            .WithButton(eternalButton), null, null).Create());
                        Thread.Sleep(1000);
                    }
                    Stuff("Are you lost in the sea of channels and cant find your way through them? " +
                          "Check out the `Server Directory` button. It provides information on all the " +
                          "channels and when to use them.",
                           EternalButtons.EternalButton1);
                    
                    Stuff("Do you think that the server should have something that it currently does not? " +
                          "Feel free to suggest it in the `Feedback` button. Your thoughts and suggestions go " +
                          "directly to the mod team, and we will address your concerns accordingly.",
                           EternalButtons.EternalButton2);
                    
                    Stuff("Feel like something's off with the bot? like it's doing something it shouldn't? " +
                          "Try reporting the issue using the `Bug Report` button. Bug reports are highly " +
                          "appreciated by the developer team, and it helps make everyone's experience better",
                           EternalButtons.EternalButton3);
                    
                    Stuff("Did you think of a great question but have no place to ask it? Suggest it using the " +
                          "`QOTD Suggestion` button! All suggestions go directly to the mod team, and if they think " +
                          "that it's a good question, it'll be featured in the server's next Question of the Day!",
                           EternalButtons.EternalButton4);
                } 
                    break;
            }
        }, 2));

        AddCommand(new TlcCommand("test", _ =>
        {
            var channel = RuntimeConfig.DefaultFileDump;
            // Stuff(EternalSelectMenus.GetColorRoles, "Color");
            // Stuff(EternalSelectMenus.GetPronounRoles, "Pronoun");
            // Stuff(EternalSelectMenus.GetPingRoles, "Ping");
            // Stuff(EternalSelectMenus.GetArtSpecialityRoles, "Art Spec");
            // Stuff(EternalSelectMenus.GetBotFunRoles, "Bot Fun");
            // Stuff(EternalSelectMenus.GetMiscRoles, "Misc");
            void Stuff(Func<SocketTextChannel, IEnumerable<SocketRole>> roleFunc, string prefix)
            {
                var roles = roleFunc(channel).ToArray();
                for (int i = 0; i < roles.Length; i++)
                {
                    channel.Guild.GetRole(roles[i].Id).ModifyAsync(props =>
                    {
                        props.Name = $"[{prefix}] {roles[i].Name}";
                    });
                }
            }

            // channel.SendMessageAsync(
            //     string.Join("\n", EternalSelectMenus.GetColorRoles(channel)
            //         .OrderBy(x => System.Drawing.Color.FromArgb(x.Color.R, x.Color.G, x.Color.B).GetHue())
            //         .Select(x => x.Mention)));
        }));
        
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
        
        AddCommand(new TlcCommand("getfile", args =>
        {
            TlcConsole.RunCommand($"post {Program.FileAssetsPath}\\{args[0]}");
        }, 1));
        
        AddCommand(new TlcCommand("setconfig", args =>
        {
            TlcConsole.Print(RuntimeConfig.SetSetting(args[0], args[1], out string prop)
                ? $"changed the value of {prop} to {args[1]}"
                : $"the property [{args[0]}] does not exist");
        }, 2));
        
        AddCommand(new TlcCommand("forceunlink", args =>
        {
            string[] unlinkees = args[1].Split(',').Select(x => x.ToLower().Replace(" ", "")).ToArray();

            string? DoReplace(string platform)
            {
                const string replacement = SocialMediaManager.SocialMediaUserEntry.NoLink;

                return !unlinkees.Contains(platform.ToLower().Replace(" ", "")) ? null : replacement;
            }

            Constants.Channels.Lares.TLCBetaCommandLine.SendMessageAsync(SocialMediaManager.ModifyUser(
                ulong.Parse(args[0]),
                Youtube: DoReplace("YouTube"),
                Twitter: DoReplace("Twitter"),
                DeviantArt: DoReplace("Deviantart"),
                Instagram: DoReplace("Instagram"),
                GitHub: DoReplace("GitHub"),
                Steam: DoReplace("Steam"),
                Reddit: DoReplace("Reddit"),
                ArtStation: DoReplace("ArtStation"),
                TikTok: DoReplace("TikTok"),
                Twitch: DoReplace("Twitch"),
                PersonalWebsite: DoReplace("Personal Website"))
                ? $"Unlinked {string.Join(", ", unlinkees)}"
                : "The user does not exist in the database");
        }, 2, "forcefully unlinks the selected social media platforms from a user"));
    }
    public static void AddCommand(TlcCommand command) => TlcConsole.ListCommand.Add(command);
}