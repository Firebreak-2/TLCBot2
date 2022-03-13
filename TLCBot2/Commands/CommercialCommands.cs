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
using TLCBot2.Cookies;
using TLCBot2.Core;
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
                        .WithDescription("Displays the specified HEX color.")
                        .AddOption("color-hex", ApplicationCommandOptionType.String, "the color to show", true),
                    cmd =>
                    {
                        string colorHex = (string)cmd.Data.Options.First().Value;
                
                        var color = Helper.HexCodeToColor(colorHex).DiscordColorToArgb32();
                        var inverseColor = color.Invert();
                    
                        const string endName = "TLC_Watermark.png";
                        string path = $"{Program.FileAssetsPath}{(OperatingSystem.IsWindows() ? "\\" : "/")}{endName}";
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
                    
                        var embedBuilder = new EmbedBuilder()
                            .WithColor(color.Argb32ToDiscordColor())
                            .WithImageUrl(Helper.GetFileUrl(stream, Constants.Channels.Lares.Coloore, text))
                            .WithTitle(text);
                    
                        cmd.RespondAsync(embed: embedBuilder.Build()); 
                }), guild);
            #endregion
                
            #region Scheme Command
                await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                        .WithName("scheme")
                        .WithDescription("Generate a color scheme to be used with a base color.")
                        .AddOption("base-color", ApplicationCommandOptionType.String, "The color to base the scheme off of.", true),
                    cmd =>
                    {
                        string hexcode = (string)cmd.Data.Options.First()!;
                        var color = Helper.HexCodeToColor(hexcode);
                
                        using var client = new WebClient();
                        string result = client.DownloadString($"https://www.thecolorapi.com/scheme?hex={color.ToString().Remove(0,1)}");
                
                        var matches = Regex.Matches(result, "(?<=\"hex\":{\"value\":\"#).{6}(?=\")");
                        var colors = matches.Select(x => Helper.HexCodeToColor(x.Value).DiscordColorToArgb32()).ToArray();
                
                        string outp = string.Join("\n", colors.Select(col => $"{col.Argb32ToDiscordColor()}"));
                
                        var image = new Image<Argb32>(100, 250);
                        image.FillColor((_, y) => colors[y / 50]);
                        string url = Helper.GetFileUrl(image.ToStream(), Constants.Channels.Lares.Coloore, $"```\n{outp}\n```");
                
                        var embed = new EmbedBuilder()
                            .WithTitle($"Scheme for {cmd.User.Username}")
                            .WithImageUrl(url)
                            .WithColor(color)
                            .WithFooter(outp);
                
                        cmd.RespondAsync(embed:embed.Build());
                    
                        image.Dispose();
                }), guild);
            #endregion
                
            #region Bingo Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("bingo")
                    .WithDescription("Generates a bingo board to draw from. More info on help command")
                    .AddOption("tile-count", ApplicationCommandOptionType.Integer, "the grid size",minValue:3,maxValue:11),
                cmd =>
                {
                    using var image = new Image<Argb32>(1000, 1000);
                    int tilesPerRow = cmd.Data.Options.Count > 0 ? Convert.ToInt32(cmd.Data.Options.First().Value) : 5;
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
                    
                            cmd.RespondAsync(embed:embed.Build());
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
        
                if (Helper.RandomInt(1, 20) == 1) // 1/20 chance to omit charProp
                    prompt =
                        $"{promptChars.RandomChoice()} {Regex.Replace(promptScenery.RandomChoice(), @"{.+?}", promptSceneryprops.RandomChoice())}";
                else
                    prompt =
                        $"{promptChars.RandomChoice()} {promptCharprops.RandomChoice().NamedFormat("color", promptColors.RandomChoice())} {promptScenery.RandomChoice().NamedFormat("sceneryprop",promptSceneryprops.RandomChoice())}";
        
                var embed = new EmbedBuilder()
                    .WithTitle($"Art prompt for {cmd.User.Username}")
                    .WithDescription(prompt)
                    .WithColor(Discord.Color.Blue);
                cmd.RespondAsync(embed: embed.Build());
            }), guild);
            #endregion
        
            #region Clear Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("clear")
                    .WithDescription("Deletes the number of messages specified")
                    .AddOption("amount", ApplicationCommandOptionType.Integer, "The specified amount"), 
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
                }), guild);
            #endregion
        
            #region Ping Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("ping")
                    .WithDescription("Responds with \"pong!\" to indicate that the bot is online."), 
                cmd => cmd.RespondAsync("pong!")), guild);
            #endregion
        
            #region Cookies Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("cookies")
                    .WithDescription("Check the amount of 🍪 that you own!")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to check the cookies of."),
                cmd =>
                {
                    var user = (SocketUser?)(cmd.Data.Options.Any() ? cmd.Data.Options.First().Value : null);

                    if (!CookieManager.GetUserFromDatabase(user?.Id ?? cmd.User.Id, out var cookies, out _))
                        cookies = 0;

                    var embed = new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
                        .WithTitle($"{user?.Username ?? cmd.User.Username}'s balance: {cookies}");
                
                    cmd.RespondAsync(embed:embed.Build());
                }), guild);
            #endregion
        
            #region Cookie Leaderboard Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("leaderboard")
                    .WithDescription("Shows the current 🍪 leaderboard."),
                cmd =>
                {
                    int i = 0;
                    string output = "";
                    var lines = File.ReadAllLines(CookieManager.CookieDatabasePath)
                        .OrderByDescending(x => CookieManager.DeformatUserData(x).cookies);

                    if (!lines.Any())
                    {
                        cmd.RespondAsync("Leaderboard count is currently 0, cannot function.", ephemeral:true);
                        return;
                    }
                
                    var g = cmd.Channel.GetGuild();

                    foreach (var line in lines)
                    {
                        var (userId, cookies, isBanned) = CookieManager.DeformatUserData(line);

                        if (userId == 0) continue;

                        string rankingPrefix = i switch
                        {
                            0 => "🥇",
                            1 => "🥈",
                            2 => "🥉",
                            _ => $"#{i+1}"
                        };

                        string banDash = isBanned ? "~~" : "";
                        string userMention = $"<@!{userId}>";

                        output += $"{rankingPrefix}: **{cookies}**🍪  {banDash}{userMention}{banDash}\n";
                    
                        if (++i >= 10) break;
                    }
                    output = output[..^1];

                    var embed = new EmbedBuilder()
                        .WithTitle("TLC 🍪 Leaderboard")
                        .WithColor(Discord.Color.Blue)
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
                    var count = cmd.Data.Options.Count == 2 ? Convert.ToInt32((long)cmd.Data.Options.Last().Value) : 5;

                    CookieManager.TakeOrGiveCookiesToUser(user.Id, count);
                    CookieManager.GetUserFromDatabase(user.Id, out var cookies, out _);

                    bool isPositive = count >= 0;
            
                    var embed = new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
                        .WithTitle($"{(isPositive ? "Given" : "Taken")} `{Math.Abs(count)}` 🍪 {(isPositive ? "to" : "from")} {user.Username}.")
                        .WithDescription($"current balance: {cookies}");
            
                    cmd.RespondAsync(embed:embed.Build());
                }, true), guild);
            #endregion
        
            #region Set Cookie Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("set-cookies")
                    .WithDescription("Sets the amount of 🍪 to a specific number for a specific user.")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to manipulate the 🍪 of",true)
                    .AddOption("amount", ApplicationCommandOptionType.Integer, "The set number of 🍪."),
                cmd =>
                {
                    var user = (SocketUser)cmd.Data.Options.First().Value;
                    var count = cmd.Data.Options.Count == 2 ? Convert.ToInt32((long)cmd.Data.Options.Last().Value) : 0;

                    CookieManager.GetUserFromDatabase(user.Id, out var oldCookies, out _);
                    CookieManager.AddOrEditUserToDatabase(user.Id, count);

                    var embed = new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
                        .WithTitle($"Changed the 🍪 of {user.Username}.")
                        .WithDescription($"{oldCookies} → {count}");
            
                    cmd.RespondAsync(embed:embed.Build());
                }, true), guild);
            #endregion
        }
    }
}