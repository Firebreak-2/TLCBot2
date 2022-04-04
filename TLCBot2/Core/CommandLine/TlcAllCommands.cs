using System.Diagnostics;
using System.Text.RegularExpressions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TLCBot2.Utilities;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.ApplicationComponents.Eternal;
using TLCBot2.DataManagement;

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
                    TlcConsole.Print(File.ReadAllText($"{Program.FileAssetsPath}/database.cookie"));
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
            RuntimeConfig.TLCBetaCommandLine
                .SendFileAsync(args[0], null);
        }, 1));

        {
            Dictionary<string, 
                (Func<SocketTextChannel, IEnumerable<SocketRole>> roles,
                Func<SocketTextChannel, SelectMenuBuilder> eternalMenu,
                bool canSelectMultiple)> messagesToPost = new()
            {
                {"Color Roles", (EternalSelectMenus.GetColorRoles, EternalSelectMenus.EternalSelectMenu0, false)},
                {"Pronoun Roles", (EternalSelectMenus.GetPronounRoles, EternalSelectMenus.EternalSelectMenu1, false)},
                {"Ping Roles", (EternalSelectMenus.GetPingRoles, EternalSelectMenus.EternalSelectMenu2, true)},
                {"Bot Fun Roles", (EternalSelectMenus.GetBotFunRoles, EternalSelectMenus.EternalSelectMenu3, true)},
                {"Art Specialty Roles", (EternalSelectMenus.GetArtSpecialityRoles, EternalSelectMenus.EternalSelectMenu4, true)},
                {"Misc Roles", (EternalSelectMenus.GetMiscRoles, EternalSelectMenus.EternalSelectMenu5, true)},
            };
            (string content, MessageComponent component) CreateRolesMessageData(
                string name,
                Func<SocketTextChannel, IEnumerable<SocketRole>> roles,
                Func<SocketTextChannel, SelectMenuBuilder> eternalMenu,
                bool canSelectMultiple,
                SocketTextChannel channel)
            {
                var categoricalRoles = roles(channel);
                var cont = $"**{name}**\n" + string.Join("\n",
                    categoricalRoles.Select(x => x.Mention));
                var comp = FireMessageComponent.CreateNew(
                    new FireMessageComponent(new ComponentBuilder()
                            .WithSelectMenu(eternalMenu(channel!)
                                .AddOption("Remove All", "rmv")
                                .WithMaxValues(canSelectMultiple ? categoricalRoles.Count() : 1)),
                        null, null));
                return (cont, comp);
            }

            (string content, MessageComponent component) CreateDashboardMessageData(
                string messageText,
                ButtonBuilder[] eternalButton)
            {
                var builder = new ComponentBuilder();
                foreach (var button in eternalButton)
                    builder.WithButton(button);
                return (messageText, new FireMessageComponent(builder, null, null).Create());
            }
            
            AddCommand(new("updateeternalmessage", args =>
            {
                var channel = (SocketTextChannel) Program.Client.GetChannel(ulong.Parse(args[1]));
                switch (int.Parse(args[0]))
                {
                    case 0:
                        foreach (var message in channel.GetMessagesAsync().FirstAsync().Result)
                        {
                            string key = Regex.Match(message.Content, @"(?<=(\*\*)).+?(?=\1.*)").Value;
                            if (!messagesToPost.TryGetValue(key, out var data)) continue;
                            
                            (string content, MessageComponent component) =
                                CreateRolesMessageData(key, data.roles, data.eternalMenu, data.canSelectMultiple, channel);
                            ((RestUserMessage) message).ModifyAsync(props =>
                            {
                                props.Content = content;
                                props.Components = component;
                            });
                        }
                        break;
                }
            }, 2));

            AddCommand(new TlcCommand("makeeternalmessage", args =>
            {
                var channel = (SocketTextChannel) Program.Client.GetChannel(ulong.Parse(args[1]));
                switch (int.Parse(args[0]))
                {
                    case 0:
                    {
                        void PostMessage(
                            string name,
                            Func<SocketTextChannel, IEnumerable<SocketRole>> roles,
                            Func<SocketTextChannel, SelectMenuBuilder> eternalMenu,
                            bool canSelectMultiple)
                        {
                            (string content, MessageComponent component) = 
                                CreateRolesMessageData(name, roles, eternalMenu, canSelectMultiple, channel);
                            channel.SendMessageAsync(content, components: component);
                            Thread.Sleep(1000);
                        }
                        foreach (string key in messagesToPost.Keys)
                        {
                            var data = messagesToPost.First(x => x.Key == key).Value;
                            PostMessage(key, data.roles, data.eternalMenu, data.canSelectMultiple);
                        }
                    }
                        break;
                    case 1:
                    {
                        void PostMessage(string messageText, params ButtonBuilder[] eternalButton)
                        {
                            (string content, MessageComponent component) =
                                CreateDashboardMessageData(messageText, eternalButton);
                            channel.SendMessageAsync(content, components: component);
                            Thread.Sleep(1000);
                        }

                        PostMessage("Are you lost in the sea of channels and cant find your way through them? " +
                              "Check out the `Server Directory` button. It provides information on all the " +
                              "channels and when to use them.",
                            EternalButtons.EternalButton1);

                        PostMessage("Getting confused with what the server's slash commands and message or user commands " +
                              "do? Give the `Command Catalogue` button! It will explain all there is to know " +
                              "about all of the server's custom commands.",
                            EternalButtons.EternalButton5);

                        PostMessage("Are you curious about what some server roles do or how you earn some of roles? " +
                              "The `Role Catalogue` button is here to help! It should give you and understanding " +
                              "of what most of the server's roles do and how you can get your hands on some of them.",
                            EternalButtons.EternalButton6);

                        PostMessage("Do you think that the server should have something that it currently does not? " +
                              "Using the `Feedback` button, you may access the Server Suggestions button which you " +
                              "can then use to post suggestions to the server, and members can vote whether to implement " +
                              "your suggestion or not. In there you can also find the QOTD Suggestion button, which allows " +
                              "you to suggest questions for the next Question Of The Day! And if you find any bugs with the " +
                              "TLC bot you can report the bugs using the Bug Report button. Reporting bugs of any kind is " +
                              "greatly appreciated by the developer team, and will help everyone in the long run.",
                            EternalButtons.EternalButton2);
                    }
                        break;
                }
            }, 2));
        }

        AddCommand(new TlcCommand("test", _ =>
        {
            
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
            TlcConsole.RunCommand($"post {Program.FileAssetsPath}/{args[0]}");
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

            RuntimeConfig.TLCBetaCommandLine.SendMessageAsync(SocialMediaManager.ModifyUser(
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