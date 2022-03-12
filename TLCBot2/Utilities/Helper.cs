using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using TLCBot2.Core;

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

    public static string GetFileUrl(Stream stream, SocketTextChannel? channel = null, string text = "text")
    {
        return (channel ?? Constants.Channels.Lares.DefaultFileDump)
            .SendFileAsync(stream, "ImageSend.png", text)
            ?.Result.Attachments.First().Url ?? "null";
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