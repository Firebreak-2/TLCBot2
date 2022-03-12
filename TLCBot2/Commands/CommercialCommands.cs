using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using TLCBot2.Cookies;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Commands
{
    public static class CommercialCommands
    {
        public static async Task Initialize()
        {
            var guild = Constants.Guilds.Lares!;
        
            // #region Color Command
            // await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            //         .WithName("color")
            //         .WithDescription("Displays the specified HEX color.")
            //         .AddOption("color-hex", ApplicationCommandOptionType.String, "the color to show", true),
            //     cmd =>
            //     {
            //         var colorHex = (string)cmd.Data.Options.First().Value;
            //
            //         var color = Helper.HexCodeToColor(colorHex);
            //         var inverseColor = color.Invert();
            //     
            //         const string endName = "TLC_Watermark.png";
            //         var bmp = new Bitmap(Program.FileAssetsPath + '\\' + endName);
            //     
            //         bmp.FillColor((x, y) => bmp.GetPixel(x, y) != Color.FromArgb(255, 255, 0, 255)
            //             ? Color.FromArgb(255, color.R, color.G, color.B)
            //             : Color.FromArgb(255, inverseColor.R, inverseColor.G, inverseColor.B));
            //
            //         var stream = bmp.ToStream(ImageFormat.Png);
            //         var text = $"`{color}`";
            //     
            //         var embedBuilder = new EmbedBuilder()
            //             .WithColor(color)
            //             .WithImageUrl(Helper.GetFileUrl(stream, Constants.Channels.Lares.Coloore, text))
            //             .WithTitle(text);
            //     
            //         cmd.RespondAsync(embed: embedBuilder.Build());
            //     
            //         stream.Dispose();
            //         bmp.Dispose();
            //     }), guild);
            // #endregion
        
            // #region Random Color Command
            // await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            //         .WithName("random-color")
            //         .WithDescription("Displays a random color."),
            //     cmd =>
            //     {
            //         var color = new Discord.Color(Helper.RandomInt(0, 255),Helper.RandomInt(0, 255),Helper.RandomInt(0, 255));
            //         var inverseColor = color.Invert();
            //     
            //         const string endName = "TLC_Watermark.png";
            //         var bmp = new Bitmap(Program.FileAssetsPath + '\\' + endName);
            //     
            //         bmp.FillColor((x, y) => bmp.GetPixel(x, y) != Color.FromArgb(255, 255, 0, 255)
            //             ? Color.FromArgb(255, color.R, color.G, color.B)
            //             : Color.FromArgb(255, inverseColor.R, inverseColor.G, inverseColor.B));
            //
            //         var stream = bmp.ToStream(ImageFormat.Png);
            //         var text = $"`{color}`";
            //     
            //         var embedBuilder = new EmbedBuilder()
            //             .WithColor(color)
            //             .WithImageUrl(Helper.GetFileUrl(stream, Constants.Channels.Lares.Coloore, text))
            //             .WithTitle(text);
            //     
            //         cmd.RespondAsync(embed: embedBuilder.Build());
            //     
            //         stream.Dispose();
            //         bmp.Dispose();
            //     }), guild);
            // #endregion
        
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
        
            // #region Scheme Command
            // await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            //         .WithName("scheme")
            //         .WithDescription("Generate a color scheme to be used with a base color.")
            //         .AddOption("base-color", ApplicationCommandOptionType.String, "The color to base the scheme off of.", true),
            //     cmd =>
            //     {
            //         var hexcode = (string)cmd.Data.Options.First();
            //         var color = Helper.HexCodeToColor(hexcode);
            //
            //         using var client = new WebClient();
            //         string result = client.DownloadString($"https://www.thecolorapi.com/scheme?hex={color.ToString().Remove(0,1)}");
            //
            //         var matches = Regex.Matches(result, "(?<=\"hex\":{\"value\":\"#).{6}(?=\")");
            //         var colors = matches.Select(x => Helper.HexCodeToColor(x.Value)).ToArray();
            //
            //         string outp = string.Join("\n", colors.Select(col => $"{col}"));
            //
            //         var bmp = new Bitmap(100, 250);
            //         bmp.FillColor((_, y) => colors[y / 50].DiscordColorToSystemDrawingColor());
            //         var url = Helper.GetFileUrl(bmp.ToStream(ImageFormat.Png), Constants.Channels.Lares.Coloore, $"```\n{outp}\n```");
            //
            //         var embed = new EmbedBuilder()
            //             .WithTitle($"Scheme for {cmd.User.Username}")
            //             .WithImageUrl(url)
            //             .WithColor(color)
            //             .WithFooter(outp);
            //
            //         cmd.RespondAsync(embed:embed.Build());
            //     
            //         bmp.Dispose();
            //     }), guild);
            // #endregion
        
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
        
            // #region Bingo Command
            // await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            //         .WithName("bingo")
            //         .WithDescription("Generates a bingo board to draw from. More info on help command")
            //         .AddOption("tile-count", ApplicationCommandOptionType.Integer, "the grid size",minValue:3,maxValue:9),
            //     cmd =>
            //     {
            //         var bmp = new Bitmap(1000, 1000);
            //         int tilesPerRow = cmd.Data.Options.Count > 0 ? Convert.ToInt32(cmd.Data.Options.First().Value) : 5;
            //         var bingoPrompts = File.ReadAllLines($"{Program.FileAssetsPath}\\bingoPrompts.cfg").ToHashSet();
            //
            //         // if (tilesPerRow % 2 == 0)
            //         // {
            //         //     cmd.RespondAsync("Tiles per row must be an odd number.",ephemeral:true);   
            //         //     return;
            //         // }
            //         // if (tilesPerRow >= 11)
            //         // {
            //         //     cmd.RespondAsync("Tiles cannot exceed 9x9 due to long board loading times", ephemeral: true);
            //         //     return;
            //         // }
            //
            //         const int gridLineWidth = 5;
            //         int tileWidth = bmp.Width / tilesPerRow;
            //         int tileHeight = bmp.Height / tilesPerRow;
            //
            //         bmp.FillColor((x, y) =>
            //         {
            //             for (int i = 0; i < gridLineWidth; i++)
            //             {
            //                 for (int j = 0; j < tilesPerRow; j++)
            //                 {
            //                     var val = i + bmp.Width / tilesPerRow * j;
            //                     if (val <= gridLineWidth) continue;
            //             
            //                     if (x == val || y == val) return Color.Black;
            //                 }
            //             }
            //             return Color.White;
            //         });
            //
            //         var centerPos = PointF.Empty;
            //         for (var y = 0; y < tilesPerRow; y++)
            //         {
            //             for (var x = 0; x < tilesPerRow; x++)
            //             {
            //                 var yPos = y * bmp.Height / tilesPerRow;
            //                 var xPos = x * bmp.Width  / tilesPerRow;
            //
            //                 if (x == y && x == tilesPerRow / 2)
            //                 {
            //                     centerPos = new PointF(xPos, yPos);
            //                     continue;
            //                 }
            //
            //                 var prompt = bingoPrompts.RandomChoice();
            //                 bingoPrompts.Remove(prompt);
            //                 bmp.DrawCenteredString(prompt, new Font("Arial",  prompt.Length > 8 ? 20 : 30), 
            //                     sourceRectangle:new RectangleF(
            //                         xPos, yPos,
            //                         tileWidth, tileHeight));
            //             }
            //         }
            //
            //         var imgToBeDrawn = (Bitmap)Image.FromFile($"{Program.FileAssetsPath}\\TLC_Logo.png");
            //         imgToBeDrawn = new Bitmap(imgToBeDrawn, tileWidth, tileHeight);
            //         bmp.DrawImageOnImage(imgToBeDrawn, new PointF(centerPos.X+gridLineWidth,centerPos.Y));
            //
            //         var embed = new EmbedBuilder()
            //             .WithTitle($"TLC bingo card for {cmd.User.Username}")
            //             .WithDescription(
            //                 "Draw an image that would score a bingo on the following sheet. Don't forget to shout bingo and share your finished drawing!")
            //             .WithImageUrl(Helper.GetFileUrl(bmp.ToStream(ImageFormat.Png), Constants.Channels.Lares.DefaultFileDump))
            //             .WithColor(Discord.Color.Blue);
            //
            //         cmd.RespondAsync(embed:embed.Build());
            //
            //         // bmp.Dispose();
            //         // imgToBeDrawn.Dispose();
            //     }), guild);
            // #endregion
        
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