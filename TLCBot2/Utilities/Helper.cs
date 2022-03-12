using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using TLC_Beta.Core;

namespace TLC_Beta.Utilities;

public static class Helper
{
    private static Random _rand = new();
    private static DataTable _dataTable = new();
    public static string Compute(string input) => _dataTable.Compute(input, null).ToString() ?? "null";
    public static Stream ToStream(this Image image, ImageFormat format) {
        var stream = new MemoryStream();
        image.Save(stream, format);
        stream.Position = 0;
        return stream;
    }
    public static string GoBackDirectory(string path)
    {
        return Regex.Replace(path, @"\\[^\\]+$", "");
    }

    public static string GetFileUrl(Stream stream, SocketTextChannel? channel = null, string text = "text")
    {
        return (channel ?? Constants.Channels.Lares.DefaultFileDump)
            .SendFileAsync(stream, "ImageSend.png", text)
            ?.Result.Attachments.First().Url ?? "null";
    }

    public static Color DiscordColorToSystemDrawingColor(this Discord.Color color)
    {
        return Color.FromArgb(255,color.R,color.G,color.B);
    }
    public static Discord.Color SystemDrawingColorToDiscordColor(this Color color)
    {
        return new Discord.Color(color.R,color.G,color.B);
    }
    public static Discord.Color HexCodeToColor(string hexcode)
    {
        return new Discord.Color(uint.Parse(Regex.Replace(hexcode.ToUpper(), @"[^\dA-F]", ""),
            NumberStyles.HexNumber));
    }

    public static Discord.Color Invert(this Discord.Color color)
    {
        return new Discord.Color(255 - color.R, 255 - color.G, 255 - color.B);
    }

    public static void FillColor(this Bitmap bmp, Color color) => bmp.FillColor((_,_)=>color);
    public static void FillColor(this Bitmap bmp, Func<int, int, Color> color)
    {
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                bmp.SetPixel(x, y, color(x, y));
            }
        }
    }

    public static void DrawImageOnImage(this Image img, Image imageToBeDrawn, PointF position)
    {
        var g = DefaultGraphicsFromImage(img);
        
        g.DrawImage(imageToBeDrawn, position);
        
        g.Flush();
        g.Dispose();
    }
    public static void DrawCenteredString(
        this Image img,
        string text,
        Font? font = null,
        Brush? color = null,
        StringFormat? stringFormat = null,
        RectangleF? sourceRectangle = null)
    {
        sourceRectangle ??= new RectangleF(0, 0, img.Width, img.Height);
        stringFormat ??= new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        Graphics g = DefaultGraphicsFromImage(img);

        g.DrawString(text, font ?? new Font("Arial",12), color ?? Brushes.Black, sourceRectangle.Value, stringFormat);
        
        g.Flush();
        g.Dispose();
    }

    public static Graphics DefaultGraphicsFromImage(Image img)
    {
        Graphics g = Graphics.FromImage(img);
        
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        return g;
    }

    public static int RandomInt(int min, int max) => _rand.Next(min, max + 1);

    public static T RandomChoice<T>(this IEnumerable<T> collection) =>
        collection.ToArray()[RandomInt(0, collection.Count() - 1)];

    public static string NamedFormat(this string baseString, string key, string replacement)
    {
        return Regex.Replace(baseString, $"{{{key}}}", replacement);
    }
    public static string Format(this string baseString, params string[] replacement)
    {
        string output = baseString;
        for (int i = 0; i < replacement.Length; i++)
        {
            output = Regex.Replace(output, $"{{{i}}}", replacement[i]);
        }
        return output;
    }
    public static SocketGuild GetGuild(this ISocketMessageChannel channel) => Program.Client.Guilds.First(x => x.Channels.Any(y => y.Id == channel.Id));
}