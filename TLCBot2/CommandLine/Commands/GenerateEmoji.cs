using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using Discord;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TLCBot2.ApplicationCommands;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Utilities;
using Color = SixLabors.ImageSharp.Color;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    private static readonly Regex s_validHexRegex = new(@"^#?([0-9A-F]{6})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    [TerminalCommand(Description = "Generates a colored emoji in the backend server")]
    public static async Task GenerateEmoji(string colorHex, bool send = true, bool noUpdate = false)
    {
        var match = s_validHexRegex.Match(colorHex);
        if (!match.Success)
        {
            await ChannelTerminal.PrintAsync($"[{colorHex}] is not a valid color in hex");
            return;
        }

        var guild = RuntimeConfig.BackendServer!;
        var emotes = guild.Emotes;

        if (emotes.Count >= guild.GetEmoteLimit())
        {
            await ChannelTerminal.PrintAsync("The backend server has reached the emote limit");
            return;
        }
        
        Discord.Color color = new(uint.Parse(match.Groups[1].Value, NumberStyles.HexNumber));
        
        using var image = new Image<Argb32>(200, 200);
        image.FillColor(color.DiscordColorToArgb32());
        image.Mutate(img =>
        {
            img.ApplyRoundedCorners(50f);
        });
        await using var stream = image.ToStream();
        string name = "e_" + color.ToString()[1..];

        GuildEmote? emote = null;
        
        if (!noUpdate && emotes.TryFirst(x => x.Name == name, out emote))
        {
            await guild.DeleteEmoteAsync(emote);
            emote = null;
        }

        emote ??= await guild.CreateEmoteAsync(name, new Discord.Image(stream));

        if (send)
            await ChannelTerminal.Channel.SendMessageAsync(emote.Mention());
    }
    
    // [ DISCLAIMER ]
    // not my code below
    // source: https://github.com/SixLabors/Samples/blob/main/ImageSharp/AvatarWithRoundedCorner/Program.cs
    
    public static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
    {
        Size size = ctx.GetCurrentSize();
        IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

        ctx.SetGraphicsOptions(new GraphicsOptions
        {
            Antialias = true,
            AlphaCompositionMode = PixelAlphaCompositionMode.DestOut 
        });
        
        foreach (var c in corners)
        {
            ctx = ctx.Fill(Color.Red, c);
        }
        return ctx;
    }
    
    private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
    {
        var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

        IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

        float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
        float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

        IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
        IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
        IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

        return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
    }
}