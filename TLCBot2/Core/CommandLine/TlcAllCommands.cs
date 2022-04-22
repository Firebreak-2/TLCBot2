using System.Diagnostics;
using System.Text.RegularExpressions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using MoreLinq;
using TLCBot2.Utilities;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.ApplicationComponents.Eternal;
using TLCBot2.DataManagement;
using TLCBot2.DataManagement.Cookies;

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
            Helper.Kill();
        }));

        AddCommand(new TlcCommand("restart", _ =>
        {
            TlcConsole.Print("Attempting restart");
            Helper.RestartProgram();
        }));

        AddCommand(new TlcCommand("allrolebans", _ =>
        {
            var items = UserRoleBans.LoadAll();
            if (items.Any())
                RuntimeConfig.TLCBetaCommandLine.SendMessageAsync(
                    string.Join("\n", items.Select((x, i) =>
                        $"{i + 1} | (`User:` <@!{x.UserID}>, " +
                        $"`Roles:` {string.Join(", ", x.RolesBannedIDs.Select(role => $"<@&{role}>"))})")),
                    allowedMentions: AllowedMentions.None);
            else
                TlcConsole.Print("There are no role bans");
        }));
        AddCommand(new TlcCommand("addroleban", args =>
        {
            var roleBan = new UserRoleBans.RoleBan(
                args[0].To<ulong>(),
                args[1].Split(',').Select(ulong.Parse).ToArray());
            var roles = roleBan.RolesBannedIDs.Select(Helper.GetRoleFromId);

            if (RuntimeConfig.TLCBetaCommandLine.Guild.GetUser(roleBan.UserID) is not { } selectedUser || roles.Any(x => x == null)) return;
            
            UserRoleBans.Add(roleBan);
            TlcConsole.Print($"Added new role ban with the user id of [{roleBan.UserID}]");
            selectedUser.RemoveRolesAsync(roles);
        }, 2));
        AddCommand(new TlcCommand("removeroleban", args =>
        {
            UserRoleBans.RemoveAll(x => x.UserID.ToString() == args[0]);
            TlcConsole.Print($"Removed all role bans with the user id of [{args[0]}]");
        }, 1));

        AddCommand(new TlcCommand("allreminders", _ =>
        {
            var items = BotMessageReminders.LoadAll();
            if (items.Any())
                RuntimeConfig.TLCBetaCommandLine.SendMessageAsync(
                    string.Join("\n", items.Select((x, i) => 
                    $"{i+1} | (`ID:` {x.ReminderID}, " +
                    $"`Channel:` <#{x.ChannelID}>, " +
                    $"`Span:` {x.Delay.Humanize()}, " +
                    $"`Message:` {x.Message})")));
            else 
                TlcConsole.Print("There are no reminders");
        }));
        AddCommand(new TlcCommand("addreminder", strVal =>
        {
            string[] args = strVal.First().Split(',').Select(x => x.Trim()).ToArray();

            var reminder = new BotMessageReminders.BotReminder(
                args[0],
                ulong.Parse(args[1]),
                TimeSpan.FromMinutes(int.Parse(args[2])), // delay in minutes
                args[3]);

            if (Program.Client.GetChannel(reminder.ChannelID) == null)
                throw new Exception("Channel provided does not exist");
            
            BotMessageReminders.Add(reminder);
            TlcConsole.Print($"Added new reminder with the id of [{reminder.ReminderID}]");
        }, 1));
        AddCommand(new TlcCommand("removereminder", args =>
        {
            BotMessageReminders.RemoveAll(x => x.ReminderID == args[0]);
            TlcConsole.Print($"Removed all reminders with the id of [{args[0]}]");
        }, 1));
        
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
                {"Pronoun Roles", (EternalSelectMenus.GetPronounRoles, EternalSelectMenus.EternalSelectMenu1, true)},
                {"Ping Roles", (EternalSelectMenus.GetPingRoles, EternalSelectMenus.EternalSelectMenu2, true)},
                {"Bot Fun Roles", (EternalSelectMenus.GetBotFunRoles, EternalSelectMenus.EternalSelectMenu3, true)},
                {"Art Specialty Roles", (EternalSelectMenus.GetArtSpecialityRoles, EternalSelectMenus.EternalSelectMenu4, true)},
                {"Misc Roles", (EternalSelectMenus.GetMiscRoles, EternalSelectMenus.EternalSelectMenu5, true)},
                {"Commission Status Roles", (EternalSelectMenus.GetCommissionStatusRoles, EternalSelectMenus.EternalSelectMenu6, false)},
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
                            .WithSelectMenu(eternalMenu(channel)
                                .AddOption("Remove All", "rmv")
                                .WithMaxValues(canSelectMultiple ? categoricalRoles.Count() + 1 : 1)),
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
            
            AddCommand(new TlcCommand("updateeternalmessage", args =>
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
            
            AddCommand(new TlcCommand("test", _ =>
            {
                
            }));
            
            AddCommand(new TlcCommand("lockserver", _ =>
            {
                var guild = RuntimeConfig.TLCBetaCommandLine.Guild;
                guild.EveryoneRole.Permissions.Modify(viewChannel: false);
                RuntimeConfig.MaintenanceModeChannel.GetPermissionOverwrite(guild.EveryoneRole)!.Value
                    .Modify(viewChannel: PermValue.Allow);
                TlcConsole.Print("Server locked");
            }));
            
            AddCommand(new TlcCommand("unlockserver", _ =>
            {
                var guild = RuntimeConfig.TLCBetaCommandLine.Guild;
                guild.EveryoneRole.Permissions.Modify(viewChannel: true);
                RuntimeConfig.MaintenanceModeChannel.GetPermissionOverwrite(guild.EveryoneRole)!.Value
                    .Modify(viewChannel: PermValue.Deny);
                TlcConsole.Print("Server unlocked");
            }));

            AddCommand(new TlcCommand("addswitchbutton", args =>
            {
                SocketTextChannel channel = (Program.Client.GetChannel(ulong.Parse(args[0])) as SocketTextChannel)!;
                string[] options = args[1].Split(',').Select(x => x.Trim()).ToArray();

                string outputData = 
                    $"Current: {options[0]}\n\n```{string.Join("\n", options.Select((x, i) => $"{i} | {x}"))}```";
                
                channel.SendMessageAsync(outputData, components: 
                    new FireMessageComponent(new ComponentBuilder()
                        .WithButton(EternalButtons.EternalButton3),
                        null,
                        null)
                        .Create());
            }, 2));
            
            AddCommand(new TlcCommand("createdashboard", _ =>
            {
                var channel = RuntimeConfig.DashboardChannel;
                void PostMessage(string messageText, params ButtonBuilder[] eternalButton)
                {
                    (string content, MessageComponent component) = CreateDashboardMessageData(messageText, eternalButton);

                    channel.SendMessageAsync(content, components: component);
                    Thread.Sleep(1000);
                }
                PostMessage("Are lost in the sea of channels and cant find your way through them? " +
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
            }));
            AddCommand(new TlcCommand("updatemoddashboard", _ =>
            {
                
            }));

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
                            channel.SendMessageAsync(content, components: component, allowedMentions: AllowedMentions.None);
                            Thread.Sleep(1000);
                        }
                        foreach (string key in messagesToPost.Keys)
                        {
                            var data = messagesToPost.First(x => x.Key == key).Value;
                            PostMessage(key, data.roles, data.eternalMenu, data.canSelectMultiple);
                        }
                    }
                        break;
                }
            }, 2));
        }

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