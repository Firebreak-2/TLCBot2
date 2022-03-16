using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Utilities;
using SkiaSharp;
using TLCBot2.Core;
using TLCBot2.DataManagement;
using Color = Discord.Color;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.Commands
{
    public static class CommercialCommands
    {
        public static async Task Initialize()
        {
            var guild = Constants.Guilds.Lares!;
        
            #region Color Command
                await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                        .WithName("color")
                        .WithDescription("Displays the specified color.")
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("hex")
                            .WithDescription("The hexadecimal value to interpret the color")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("value", ApplicationCommandOptionType.String, "the hex value", true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("rgb")
                            .WithDescription("The RGB value to interpret the color")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("red")
                                .WithDescription("The red color value")
                                .WithType(ApplicationCommandOptionType.Integer)
                                .WithRequired(true)
                                .WithMinValue(0)
                                .WithMaxValue(255))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("green")
                                .WithDescription("The green color value")
                                .WithType(ApplicationCommandOptionType.Integer)
                                .WithRequired(true)
                                .WithMinValue(0)
                                .WithMaxValue(255))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("blue")
                                .WithDescription("The blue color value")
                                .WithType(ApplicationCommandOptionType.Integer)
                                .WithRequired(true)
                                .WithMinValue(0)
                                .WithMaxValue(255)
                            )),
                        // .AddOption("color-hex", ApplicationCommandOptionType.String, "the color to show", true),
                    cmd =>
                    {
                        var colorType = (SocketSlashCommandDataOption)cmd.Data.Options.First();
                
                        Argb32 color;
                        switch (colorType.Name)
                        {
                            case "hex":
                                string colorHex = (string) colorType.Options.First().Value;
                                color = Helper.HexCodeToColor(colorHex).DiscordColorToArgb32();
                                break;
                            case "rgb":
                                byte colorR = Convert.ToByte((long) colorType.Options.ToArray()[0].Value);
                                byte colorG = Convert.ToByte((long) colorType.Options.ToArray()[1].Value);
                                byte colorB = Convert.ToByte((long) colorType.Options.ToArray()[2].Value);
                                color = new Color(colorR, colorG, colorB).DiscordColorToArgb32();
                                break;
                            default:
                                throw new Exception("how??!?!");
                        }
                        var inverseColor = color.Invert();
                    
                        const string endName = "TLC_Watermark.png";
                        string path = $"{Program.FileAssetsPath}\\{endName}";
                        using Image<Argb32> image = Image.Load<Argb32>(path);
    
                        var magenta = new Argb32(255, 0, 255);
                        image.FillColor((_, _, pixel) => pixel != magenta
                            ? color
                            : inverseColor);
                
                        using var stream = image.ToStream();
                        string text = $"`{color}`\n`{color.Argb32ToDiscordColor()}`";
                    
                        var embedBuilder = new EmbedBuilder()
                            .WithColor(color.Argb32ToDiscordColor())
                            .WithImageUrl(Helper.GetFileUrl(stream, Constants.Channels.Lares.Coloore, text))
                            .WithTitle(text);
                    
                        cmd.RespondAsync(embed: embedBuilder.Build());
                }), guild);
            #endregion
            
            #region Random Color Command
                await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                        .WithName("random-color")
                        .WithDescription("Displays a random color."),
                    cmd =>
                    {
                        Embed GetColorEmbed()
                        {
                            var color = new Color(Helper.RandomInt(0, 255),Helper.RandomInt(0, 255),Helper.RandomInt(0, 255))
                                .DiscordColorToArgb32();
                            var inverseColor = color.Invert();
                    
                            const string endName = "TLC_Watermark.png";
                            string path = Program.FileAssetsPath + '\\' + endName;
                            using Image<Argb32> image = Image.Load<Argb32>(path);
    
                            var magenta = new Argb32(255, 0, 255);
                            image.FillColor((_, _, pixel) => pixel != magenta
                                ? color
                                : inverseColor);
                
                            using var stream = image.ToStream();
                            string text = $"`{color}`\n`{color.Argb32ToDiscordColor()}`";
                    
                            return new EmbedBuilder()
                                .WithColor(color.Argb32ToDiscordColor())
                                .WithImageUrl(Helper.GetFileUrl(stream, Constants.Channels.Lares.Coloore, text))
                                .WithTitle(text)
                                .Build();
                        }
                        var embedBuilder = GetColorEmbed();

                        var fmc = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                            .WithButton("Reroll", $"reroll-randcol-{Helper.RandomInt(0, 1000)}"), button =>
                        {
                            button.Message.ModifyAsync(props => props.Embed = GetColorEmbed());
                            button.RespondAsync();
                        }, null));
                    
                        cmd.RespondAsync(embed: embedBuilder, components:fmc); 
                }), guild);
            #endregion
                
            #region Scheme Command
                await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                        .WithName("scheme")
                        .WithDescription("Generate a color scheme to be used with a base color.")
                        .AddOption("base-color", ApplicationCommandOptionType.String, "The color to base the scheme off of.", true),
                    cmd =>
                    {
                        Embed GetSchemeEmbed()
                        {
                            string hexcode = (string) cmd.Data.Options.First()!;
                            var color = Helper.HexCodeToColor(hexcode);

                            using var client = new WebClient();
                            string result =
                                client.DownloadString(
                                    $"https://www.thecolorapi.com/scheme?hex={color.ToString().Remove(0, 1)}");

                            var matches = Regex.Matches(result, "(?<=\"hex\":{\"value\":\"#).{6}(?=\")");
                            var colors = matches.Select(x => Helper.HexCodeToColor(x.Value).DiscordColorToArgb32())
                                .ToArray();

                            string outp = string.Join("\n", colors.Select(col => $"{col.Argb32ToDiscordColor()}"));

                            using var image = new Image<Argb32>(100, 250);
                            image.FillColor((_, y) => colors[y / 50]);
                            string url = Helper.GetFileUrl(image.ToStream(), Constants.Channels.Lares.Coloore,
                                $"```\n{outp}\n```");

                            return new EmbedBuilder()
                                .WithTitle($"Scheme for {cmd.User.Username}")
                                .WithImageUrl(url)
                                .WithColor(color)
                                .WithFooter(outp)
                                .Build();
                        }
                        cmd.RespondAsync(embed:GetSchemeEmbed());
                }), guild);
            #endregion
                
            #region Bingo Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("bingo")
                    .WithDescription("Generates a bingo board to draw from. More info on help command")
                    .AddOption("tile-count", ApplicationCommandOptionType.Integer, "the grid size",minValue:3,maxValue:11),
                cmd =>
                {
                    Embed GetBingoEmbed()
                    {
                        using var image = new Image<Argb32>(1000, 1000);
                        int tilesPerRow = cmd.Data.Options.Count > 0
                            ? Convert.ToInt32(cmd.Data.Options.First().Value)
                            : 5;
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
                                int xPos = x * image.Width / tilesPerRow;

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

                        return new EmbedBuilder()
                            .WithTitle($"TLC bingo card for {cmd.User.Username}")
                            .WithDescription(
                                "Draw an image that would score a bingo on the following sheet. Don't forget to shout bingo and share your finished drawing!")
                            .WithImageUrl(Helper.GetFileUrl(image.ToStream(), Constants.Channels.Lares.DefaultFileDump))
                            .WithColor(Color.Blue)
                            .Build();
                    }
                    
                    var fmc = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                        .WithButton("Reroll", $"reroll-bingo-{Helper.RandomInt(0, 1000)}"), button =>
                    {
                        button.Message.ModifyAsync(props => props.Embed = GetBingoEmbed());
                        button.RespondAsync();
                    }, null));

                    cmd.RespondAsync(embed:GetBingoEmbed(),components:fmc);
                    }), guild);
        #endregion
        
            #region Random Prompt Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                .WithName("prompt")
                .WithDescription("Generates a random prompt for you to draw!"), cmd =>
            {
                string prompt;
            
                string[] promptCharprops =    File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\charProps.cfg");
                string[] promptChars =        File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\chars.cfg");
                string[] promptColors =       File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\colors.cfg");
                string[] promptScenery =      File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\scenery.cfg");
                string[] promptSceneryprops = File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\sceneryProps.cfg");

                Embed GetRandomPromptEmbed()
                {

                    if (Helper.RandomInt(1, 20) == 1) // 1/20 chance to omit charProp
                        prompt =
                            $"{promptChars.RandomChoice()} {Regex.Replace(promptScenery.RandomChoice(), @"{.+?}", promptSceneryprops.RandomChoice())}";
                    else
                        prompt =
                            $"{promptChars.RandomChoice()} {promptCharprops.RandomChoice().NamedFormat("color", promptColors.RandomChoice())} {promptScenery.RandomChoice().NamedFormat("sceneryprop", promptSceneryprops.RandomChoice())}";

                    return new EmbedBuilder()
                        .WithTitle($"Art prompt for {cmd.User.Username}")
                        .WithDescription($"{prompt[0].ToString().ToUpper()}{prompt[1..]}.")
                        .WithColor(Color.Blue)
                        .Build();
                }
                
                var fmc = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                    .WithButton("Reroll", $"reroll-prompt-{Helper.RandomInt(0, 1000)}"), button =>
                {
                    button.Message.ModifyAsync(props => props.Embed = GetRandomPromptEmbed());
                    button.RespondAsync();
                }, null));

                cmd.RespondAsync(embed:GetRandomPromptEmbed(),components:fmc);
            }), guild);
            #endregion
        
            #region Clear Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("clear")
                    .WithDescription("Deletes the number of messages specified")
                    .AddOption("amount", ApplicationCommandOptionType.Integer, "The specified amount", true), 
                cmd =>
                {
                    long count = (long) cmd.Data.Options.First().Value;
                    switch (count)
                    {
                        case <= 0:
                            cmd.RespondAsync("You cant delete something that doesn't exist??", ephemeral:true);
                            return;
                        case > 50:
                            cmd.RespondAsync("dude.. that's wayyy too many channels. ", ephemeral:true);
                            return;
                    }
        
                    var messages = cmd.Channel.GetMessagesAsync((int)count, CacheMode.AllowDownload, RequestOptions.Default)
                        .ToArrayAsync().Result.First().ToArray();
        
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            cmd.Channel.DeleteMessageAsync(messages[i], RequestOptions.Default);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        cmd.RespondAsync("An error has occured. If this problem prevails, contact `Firebreak#3813`.",
                            ephemeral: true);
                        return;
                    }
        
                    cmd.RespondAsync($"`{count}` messages deleted.", ephemeral: true);
                }, true), guild);
            #endregion
        
            #region Ping Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("ping")
                    .WithDescription("Responds with \"pong!\" to indicate that the bot is online."), 
                cmd => cmd.RespondAsync("pong!")), guild);
            #endregion
        
            #region Cookies Command
            await FireCommand.CreateNew(new(new SlashCommandBuilder()
                    .WithName("cookies")
                    .WithDescription("Check the amount of 🍪 that you own!")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to check the cookies of."),
                cmd =>
                {
                    var user = (SocketUser?)(cmd.Data.Options.Any() ? cmd.Data.Options.First().Value : null) ?? cmd.User;

                    int cookies = 0;
                    if (CookieManager.GetUser(user.Id, out var entry))
                        cookies = entry.Cookies;

                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle($"{user.Username}'s balance: {cookies}");
                
                    cmd.RespondAsync(embed:embed.Build());
                }), guild);
            #endregion
        
            #region Cookie Leaderboard Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("leaderboard")
                    .WithDescription("Shows the current 🍪 leaderboard."),
                cmd =>
                {
                    string output = "No users registered in leaderboard";
                    if (CookieManager.CookieUsers.Any())
                        output = string.Join("\n", CookieManager.CookieUsers
                            .Where((_, i) => i < 10).Select((user, i) =>
                        {
                            string rankingPrefix = i switch
                            {
                                0 => "🥇",
                                1 => "🥈",
                                2 => "🥉",
                                _ => $"#{i+1}"
                            };
                            string banDash = user.IsBanned ? "~~" : "";
                            string userMention = $"<@!{user.UserID}>";
                            return $"{rankingPrefix}: **{user.Cookies}**🍪  {banDash}{userMention}{banDash}";
                        }));
                    
                    var embed = new EmbedBuilder()
                        .WithTitle("TLC 🍪 Leaderboard")
                        .WithColor(Color.Blue)
                        .WithDescription(output)
                        .WithCurrentTimestamp();
                    
                    cmd.RespondAsync(embed:embed.Build());
                }), guild);
            #endregion
        
            #region Give Cookie Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("give-cookie")
                    .WithDescription("Adds or removes(using negatives) 🍪 from a user.")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to manipulate the 🍪 of",true)
                    .AddOption("count", ApplicationCommandOptionType.Integer, "The amount of 🍪 to give to the person."),
                cmd =>
                {
                    var user = (SocketUser)cmd.Data.Options.First().Value;
                    int count = cmd.Data.Options.Count == 2 ? Convert.ToInt32((long)cmd.Data.Options.Last().Value) : 5;

                    CookieManager.TakeOrGiveCookiesToUser(user.Id, count);
                    CookieManager.GetUser(user.Id, out var entry);

                    bool isPositive = count >= 0;
            
                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle($"{(isPositive ? "Given" : "Taken")} `{Math.Abs(count)}` 🍪 {(isPositive ? "to" : "from")} {user.Username}.")
                        .WithDescription($"current balance: {entry.Cookies}");
            
                    cmd.RespondAsync(embed:embed.Build());
                }, true), guild);
            #endregion
        
            #region Set Cookie Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("set-cookies")
                    .WithDescription("Sets the amount of 🍪 to a specific number for a specific user.")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to manipulate the 🍪 of",true)
                    .AddOption("amount", ApplicationCommandOptionType.Integer, "The set number of 🍪.", true),
                cmd =>
                {
                    var user = (SocketUser)cmd.Data.Options.First().Value;
                    int count = cmd.Data.Options.Count == 2 ? Convert.ToInt32((long)cmd.Data.Options.Last().Value) : 0;

                    CookieManager.GetUser(user.Id, out var entry);
                    CookieManager.AddOrModifyUser(user.Id, count);

                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle($"Changed the 🍪 of {user.Username}.")
                        .WithDescription($"{entry?.Cookies ?? 0} → {count}");
            
                    cmd.RespondAsync(embed:embed.Build());
                }, true), guild);
            #endregion
        }
    }
}