using System.Data;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TLCBot2.Core;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.Utilities;

public static class Helper
{
    private static Random _rand = new();
    private static DataTable _dataTable = new();
    public static string Compute(string input) => _dataTable.Compute(input, null).ToString() ?? "null";
    public static string GoBackDirectory(string path)
    {
        return Regex.Replace(path, @"\\[^\\]+$", "");
    }
    public static string GetFileNameFromPath(string path)
    {
        return Regex.Match(path, @"\\[^\\]+$").Value;
    }

    public static bool CheckStringIsLink(string link) =>
        Regex.IsMatch(link,
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
    public static string GetFileUrl(Stream stream, SocketTextChannel? channel = null, string text = "text")
    {
        return (channel ?? Constants.Channels.Lares.DefaultFileDump)
            .SendFileAsync(stream, "ImageSend.png", text)
            ?.Result.Attachments.First().Url ?? "null";
    }
    public static Stream ToStream(this Image image) 
    {
        var stream = new MemoryStream();
        image.SaveAsPng(stream);
        stream.Position = 0;
        return stream;
    }
    public static void FillColor(this Image<Argb32> img, Argb32 color) => img.FillColor((_,_)=>color);
    public static void FillColor(this Image<Argb32> img, Func<int, int, Argb32> color) => img.FillColor((x, y, _) => color(x, y));
    public static void FillColor(this Image<Argb32> img, Func<int, int, Argb32, Argb32> color)
    {
        img.ProcessPixelRows(pixelAccessor =>
        {
            for (int y = 0; y < img.Height; y++)
            {
                Span<Argb32> pixelRow = pixelAccessor.GetRowSpan(y);
                for (int x = 0; x < img.Width; x++)
                {
                    var prevCol = pixelRow[x];
                    pixelRow[x] = color(x, y, prevCol);
                }
            }
        });
    }
    public static Discord.Color HexCodeToColor(string hexcode)
    {
        return new Discord.Color(uint.Parse(Regex.Replace(hexcode.ToUpper()[..6], @"[^\dA-F]", ""),
            NumberStyles.HexNumber));
    }

    public static Discord.Color Invert(this Discord.Color color)
    {
        return new Discord.Color(255 - color.R, 255 - color.G, 255 - color.B);
    }

    public static Argb32 Invert(this Argb32 color)
    {
        return color.Argb32ToDiscordColor().Invert().DiscordColorToArgb32();
    }
    public static Discord.Color Argb32ToDiscordColor(this Argb32 color) => new(color.R, color.G, color.B);
    public static Argb32 DiscordColorToArgb32(this Discord.Color color) => new(color.R, color.G, color.B, 255);
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

    public static string GetRandomWord()
    {
        using var client = new WebClient();
        
        return Regex.Match(
            client.DownloadString("https://randomword.com/"),
            "(?<=<div id=\"random_word\">).+(?=</div>)").Value;
    }
    public static SocketGuild GetGuild(this ISocketMessageChannel channel) => Program.Client.Guilds.First(x => x.Channels.Any(y => y.Id == channel.Id));
}