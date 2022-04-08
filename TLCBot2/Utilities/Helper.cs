using System.Data;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TLCBot2.Core;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.Utilities;

public static class Helper
{
     // public static class Sheets
     // {
     //     public static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
     //     public static string ApplicationName = "Google Sheets API .NET Quickstart";
     //     public static UserCredential Credential = null!;
     //     public static SheetsService  Service    = null!;
     //     public const string SpreadsheetId = "null";
     //     public static void Initialize()
     //     {
     //         using (var stream =
     //                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
     //         {
     //             const string credPath = "token.json";
     //             Credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
     //                 GoogleClientSecrets.FromStream(stream).Secrets,
     //                 Scopes,
     //                 "user",
     //                 CancellationToken.None,
     //                 new FileDataStore(credPath, true)).Result;
     //             Console.WriteLine($"Credential file saved to: {credPath}");
     //         }
     //         
     //         Service = new SheetsService(new BaseClientService.Initializer
     //         {
     //             HttpClientInitializer = Credential,
     //             ApplicationName = ApplicationName
     //         });
     //     }
     //     public static string UpdateData(string rangeNotation, IList<IList<object>> data)
     //     {
     //         const string valueInputOption = "USER_ENTERED";
     //
     //         // The new values to apply to the spreadsheet.
     //         List<ValueRange> updateData = new();
     //         var dataValueRange = new ValueRange
     //         {
     //             Range = $"{rangeNotation}",
     //             Values = data
     //         };
     //         updateData.Add(dataValueRange);
     //
     //         BatchUpdateValuesRequest requestBody = new()
     //         {
     //             ValueInputOption = valueInputOption,
     //             Data = updateData
     //         };
     //
     //         var request = Service.Spreadsheets.Values.BatchUpdate(requestBody, SpreadsheetId);
     //
     //         BatchUpdateValuesResponse response = request.Execute();
     //         // BatchUpdateValuesResponse response = await request.ExecuteAsync(); // For async 
     //
     //         return JsonConvert.SerializeObject(response);
     //     }
     //     public static string UpdateCell(string cellID, string newValue) =>
     //         UpdateData(cellID, new List<IList<object>> { new List<object> { newValue } });
     //     public static IList<IList<object>> GetData(string a1Notation) =>
     //         Service.Spreadsheets.Values.Get(SpreadsheetId, a1Notation).Execute().Values;
     //     public static string GetCell(string cellID) => GetData(cellID)[0][0].ToString() ?? "";
     // }
    private static Random _rand = new();
    private static DataTable _dataTable = new();
    public static string Compute(string input) => _dataTable.Compute(input, null).ToString() ?? "null";
    public static string GoBackDirectory(string path)
    {
        return Regex.Replace(path, @"/[^/]+$", "");
    }
    public static string GetFileNameFromPath(string path)
    {
        return Regex.Match(path, @"/[^/]+$").Value;
    }

    public static bool CheckStringIsLink(string link) =>
        Regex.IsMatch(link,
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
    public static string GetFileUrl(Stream stream, SocketTextChannel? channel = null, string text = "text")
    {
        channel ??= RuntimeConfig.DefaultFileDump;
        var msg = channel.SendFileAsync(stream, "ImageSend.png", text).Result;
        return msg.Attachments?.FirstOrDefault()?.Url ?? "null";
    }

    public static Image<Argb32> ImageFromUrl(string url)
    {
        using var client = new WebClient();
        byte[] data = client.DownloadData(new Uri(url));
        return Image.Load<Argb32>(data);
    }
    public static string RandId(string name) => $"{name}-{RandomInt(int.MinValue, int.MaxValue - 1)}";
    public static void DisableMessageComponents(SocketUserMessage message)
    {
        if (message.Author.Id != Program.Client.CurrentUser.Id) return;
        message.ModifyAsync(props =>
        {
            var componentBuilder = new ComponentBuilder();
            foreach (var actionRow in message.Components)
            {
                var row = new ActionRowBuilder();
                foreach (var component in actionRow.Components)
                {
                    if (component.Type == ComponentType.Button)
                    {
                        row.AddComponent(((ButtonComponent) component)
                            .ToBuilder()
                            .WithDisabled(true)
                            .WithStyle(ButtonStyle.Secondary).Build());
                    }
                    else if (component.Type == ComponentType.SelectMenu)
                    {
                        row.AddComponent(((SelectMenuComponent) component)
                            .ToBuilder()
                            .WithDisabled(true).Build());
                    }
                    else return;
                }
                componentBuilder.AddRow(row);
            }
            props.Components = componentBuilder.Build();
        });
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

    public static T GetRequiredValue<T>(this SocketSlashCommand cmd, string optionName)
    {
        object value = cmd.Data.Options.First(x => x.Name == optionName).Value!;
        return (T) (value is long ? Convert.ToInt32(value) : value);
    }
    public static T GetOptionalValue<T>(this SocketSlashCommand cmd, string optionName, T defaultValue)
    {
        if (cmd.Data.Options.Any(x => x.Name == optionName))
        {
            object value = cmd.Data.Options.First(x => x.Name == optionName).Value!;
            return (T)(value is long ? Convert.ToInt32(value) : value);
        }

        return defaultValue;
    }
    public static T GetRequiredValue<T>(this SocketSlashCommandDataOption cmd, string optionName)
    {
        object value = cmd.Options.First(x => x.Name == optionName).Value!;
        return (T) (value is long ? Convert.ToInt32(value) : value);
    }
    public static bool GetOption(this SocketSlashCommand cmd, string optionName, out SocketSlashCommandDataOption option)
    {
        option = null!;
        if (cmd.Data.Options.All(x => x.Name != optionName)) return false;
        {
            option = cmd.Data.Options.First(x => x.Name == optionName);
            return true;
        }
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

    public static string GetRandomWord()
    {
        using var client = new WebClient();
        
        return Regex.Match(
            client.DownloadString("https://randomword.com/"),
            "(?<=<div id=\"random_word\">).+(?=</div>)").Value;
    }
    public static SocketGuild GetGuild(this ISocketMessageChannel channel) => Program.Client.Guilds.First(x => x.Channels.Any(y => y.Id == channel.Id));
}