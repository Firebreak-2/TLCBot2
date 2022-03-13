using System.Net;
using System.Text.RegularExpressions;
using Discord;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Utilities;
using SkiaSharp;
using TLCBot2.Core;
using Color = Discord.Color;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.Commands;

public static class TestCommands
{
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;

        #region Spawn Button Command
        await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            .WithName("spawn-button")
            .WithDescription("spawns a button"), cmd =>
        {
            var cb = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                .WithSelectMenu("selectmenu-1", new List<SelectMenuOptionBuilder>
                {
                    new SelectMenuOptionBuilder()
                        .WithLabel("one")
                        .WithValue("invalid")
                        .WithDescription("desc-1"),
                    new SelectMenuOptionBuilder()
                        .WithLabel("two")
                        .WithValue("valid")
                        .WithDescription("desc-2")
                }, maxValues: 1), null, selectMenu =>
            {
                var text = string.Join(", ", selectMenu.Data.Values);
        
                Constants.Channels.Lares.DefaultFileDump
                    .SendMessageAsync($"`{selectMenu.User.Username}` selected `{text}`.\nSelected on: <{selectMenu.Message.GetJumpUrl()}>");
                
                selectMenu.RespondAsync("Response submitted.", ephemeral:true);
            }));
            
            cmd.RespondAsync(components:cb);
        }, true), guild);
        #endregion
        
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
    }
}