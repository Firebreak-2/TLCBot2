using System.Globalization;
using System.Numerics;
using Discord;
using Discord.Interactions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TLCBot2.Utilities;
using Color = Discord.Color;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [Group("color", "Displays the color inputted as an image")]
    public class ColorGroup : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("hex", "Where the value is a hexacdecimal color representation")]
        public async Task HexSubcommand(string hexCode)
        {
            hexCode = hexCode[0] == '#'
                ? hexCode[1..]
                : hexCode;
            
            if (uint.TryParse(hexCode, NumberStyles.HexNumber, null, out var num))
            {
                var color = new Color(num);
                using var image = await GenerateColorPicture(color);
                await PostImage(image, color);
                return;
            }

            await RespondAsync($"The hexcode [{hexCode}] is invalid.", ephemeral: true);
        }
        
        [SlashCommand("rgb", "Where the value is an RGB color representation")]
        public async Task RgbSubcommand(
            [MinValue(0)] [MaxValue(255)] int red,
            [MinValue(0)] [MaxValue(255)] int green,
            [MinValue(0)] [MaxValue(255)] int blue)
        {
            var color = new Color(red, green, blue);
            using var image = await GenerateColorPicture(color);
            await PostImage(image, color);
        }
        
        [SlashCommand("random", "Generates a random color")]
        public async Task RandomSubcommand()
        {
            await RgbSubcommand(Helper.Rando.Int(0, 255),
                                Helper.Rando.Int(0, 255),
                                Helper.Rando.Int(0, 255));
        }

        private static Task<Image<Argb32>> GenerateColorPicture(Color color)
        {
            var image = new Image<Argb32>(200, 200);
            image.FillColor(color.DiscordColorToArgb32());
            image.Mutate(img =>
            {
                var options = new TextOptions(Helper.ArialFont.CreateFont(50))
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Origin = new Vector2(195, 200)
                };
                img.DrawText(options, "TLC", color.Invert().DiscordColorToArgb32());
            });

            return Task.FromResult(image);
        }

        private async Task PostImage(Image image, Color color)
        {
            await RespondAsync(embed: new EmbedBuilder()
                .WithImageUrl(await Helper.GetFileUrl(image.ToStream()))
                .WithColor(color)
                .WithTitle(color.ToString())
                .Build());
        }
    }
}