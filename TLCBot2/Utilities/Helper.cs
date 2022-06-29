using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TLCBot2.Core;
using Image = SixLabors.ImageSharp.Image;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TLCBot2.Utilities;

public static partial class Helper
{
    private static Random _rand = new();
    private static DataTable _dataTable = new();
    public static string Compute(string input) => _dataTable.Compute(input, null).ToString() ?? "null";
    public static string GoBackDirectory(string path)
    {
        return Regex.Replace(path, @"/[^/]+$", "");
    }

    public static string GetHyperLink(this string text, string link) => $"[{text}]({link})";
    public static string GetFileNameFromPath(string path)
    {
        return Regex.Match(path, @"/[^/]+$").Value;
    }

    private static readonly string _ansiBlueBg = Ansi.Generate(null, Ansi.Background.Indigo);
    private static readonly string _ansiYellowFg = Ansi.Generate(Ansi.Foreground.Yellow);
    private static readonly string _ansiGreenFg = Ansi.Generate(Ansi.Foreground.Green);
    private static readonly string _ansiBlueFg = Ansi.Generate(Ansi.Foreground.Blue);
    private static readonly string _ansiMagentaFg = Ansi.Generate(Ansi.Foreground.Pink);
    private static readonly string _ansiCyanFg = Ansi.Generate(Ansi.Foreground.Cyan);
    private static readonly string _ansiWhiteFg = Ansi.Generate(Ansi.Foreground.White);
    
    private static readonly (Regex Regex, string color)[] _regexTokenizer =
    {
        /* multiple characters     ;  Yellow  */ (new Regex(@"(?:\[\^?(?=.*\])|(?<=\[.*)\]|(?<=\[.*)-(?=.*\]))+", RegexOptions.Compiled), _ansiYellowFg),
        /* groups & alternation    ;  Green   */ (new Regex(@"(?:(?:\(\?:|\(\?<!|\(\?!|\(\?=|\(\?<=|\(\?<\w+?>|\()(?=.+\))|(?<=(?:\(\?:|\(\?<!|\(\?!|\(\?=|\(\?<=|\(\?<\w+?>|\().+)\)|(?:(?<!\[.*(?=.*]))(?<!\\)\|(?!(?<=\[.*).*])))+", RegexOptions.Compiled), _ansiGreenFg),
        /* quantifiers             ;  Blue    */ (new Regex(@"(?:(?:\{\d+,?\d*?\}|(?<!\[.*(?=.*]))(?:\+|\*|(?<!\()\?)(?!(?<=\[.*).*])))+", RegexOptions.Compiled), _ansiBlueFg),
        /* escaped characters      ;  Magenta */ (new Regex(@"(?:\\.)+", RegexOptions.Compiled), _ansiMagentaFg),
        /* common pattern escapes  ;  Cyan    */ (new Regex(@"(?:^\^|\$$|(?<!\\)\\[wdsWDS])+", RegexOptions.Compiled), _ansiCyanFg),
    };

    public static readonly Regex AnsiFormattingRegex =
        new("\x1b\\[(?:\\d{1,2};\\d{1,2}(?:;\\d{1,2})?|0)m", RegexOptions.Compiled);

    public static string CleanAnsiFormatting(string str) => 
        AnsiFormattingRegex.Replace(str, "");

    public static string GenerateRegexHighlightedCodeBlocks([RegexPattern] string pattern, string matchString)
    {
        string finalRegexString = pattern;
        string finalParagraphString = Regex.Replace(matchString, $"(?:{pattern})+", match => $"{_ansiBlueBg}{match.Value}{Ansi.Reset}");
        foreach ((Regex regex, string color) in _regexTokenizer)
        {
            finalRegexString = regex.Replace(finalRegexString, match => $"{color}{match.Value}{_ansiWhiteFg}");
        }

        return $"```ansi\n{_ansiWhiteFg}{finalRegexString}\x1B[0m\n```\n```ansi\n{finalParagraphString}\n```";
    }
    public static string ToJson(this object obj, Formatting formatting = Formatting.Indented) =>
        JsonConvert.SerializeObject(obj, formatting);
    public static T? FromJson<T>(this string str) =>
        JsonConvert.DeserializeObject<T>(str);
    public static bool CheckStringIsLink(string link) =>
        Regex.IsMatch(link,
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
    public static string GetFileUrl(Stream stream, SocketTextChannel channel, string text = "text")
    {
        var msg = channel.SendFileAsync(stream, "ImageSend.png", text).Result;
        return msg.Attachments?.FirstOrDefault()?.Url ?? "null";
    }

    public static Image<Argb32> ImageFromUrl(string url)
    {
        using var client = new HttpClient();
        byte[] data = client.GetByteArrayAsync(new Uri(url)).Result;
        return Image.Load<Argb32>(data);
    }
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

    private static readonly Regex _autoLineBreakRegex = new(@".{0,40}(?:\s+|.$)", RegexOptions.Compiled);

    public static string ApplyLineBreaks(string str, int charsUntilLineBreak = 40)
    {
        return (charsUntilLineBreak == 40
            ? _autoLineBreakRegex.Replace(str, "$&\n")
            : Regex.Replace(str, $@".{{0,{charsUntilLineBreak}}}(?:\s+|.$)", "$&\n"))[..^1];
    }

    public static bool HasUrl(string possibleUrl, out string? url)
    {
        url = null;
        
        var match = Regex.Match(
            possibleUrl,
            @"((\w+:\/\/)[-a-zA-Z0-9:@;?&=\/%\+\.\*!'\(\),\$_\{\}\^~\[\]`#|]+)");

        if (!match.Success) return false;
        
        url = match.Value;
        return true;
    }

    public static void RestartProgram()
    {
        Console.WriteLine("RESTARTING BOT...");
        Process.Start(AppDomain.CurrentDomain.FriendlyName);
        Kill();
    }
    public static void Kill()
    {
        Environment.Exit(0);
    }

    public static T To<T>(this object thing) where T : IConvertible
    {
        return (T)Convert.ChangeType(thing, typeof(T));
    }

    public static SocketRole? GetRoleFromId(ulong Id)
    {
        foreach (var guild in Program.Client.Guilds)
        {
            if (guild.Roles.TryFirst(x => x.Id == Id, out var role))
            {
                return role;
            }
        }
        return null;
    }

    public static bool TryGetRole(ulong Id, out SocketRole? role)
    {
        role = null;
        if (GetRoleFromId(Id) is not { } r) return false;
        role = r;
        return true;
    }

    public static (string ChannelName, string ChannelType) GetChannelInfo(SocketChannel channel)
    {
        string channelTypeName = "null";
        string channelName = "null";
        switch (channel.GetChannelType())
        {
            case ChannelType.Text:
                channelTypeName = "Text Channel";
                channelName = ((SocketTextChannel) channel).Name;
                break;
            case ChannelType.DM:
                channelTypeName = "Direct Message";
                channelName = $"DM with {((SocketDMChannel) channel).Recipient.Username}";
                break;
            case ChannelType.Voice:
                channelTypeName = "Voice Channel";
                channelName = ((SocketVoiceChannel) channel).Name;
                break;
            case ChannelType.Group:
                channelTypeName = "Group Chat";
                channelName = ((SocketGroupChannel) channel).Name;
                break;
            case ChannelType.Category:
                channelTypeName = "Category";
                channelName = ((SocketCategoryChannel) channel).Name;
                break;
            case ChannelType.News:
                channelTypeName = "News Channel";
                channelName = ((SocketNewsChannel) channel).Name;
                break;
            case ChannelType.NewsThread:
            case ChannelType.PublicThread:
                channelTypeName = "Public Thread";
                channelName = ((SocketThreadChannel) channel).Name;
                break;
            case ChannelType.PrivateThread:
                channelTypeName = "Private Thread";
                channelName = ((SocketThreadChannel) channel).Name;
                break;
            case ChannelType.Stage:
                channelTypeName = "Stage Channel";
                channelName = ((SocketStageChannel) channel).Name;
                break;
        }

        return (channelName, channelTypeName);
    }
    public static IEnumerable<IUserMessage> GetLatestMessages(this SocketTextChannel channel, int limit = 100) => channel
        .GetMessagesAsync(limit)
        .ToArrayAsync()
        .Result.SelectMany(x => x)
        .OrderByDescending(x => x.Timestamp.Ticks).Cast<IUserMessage>();
    public static SocketTextChannel GetChannelFromId(ulong id) => (SocketTextChannel) Program.Client.GetChannel(id);
    public static async Task<SocketTextChannel> GetChannelFromIdAsync(ulong id) => (SocketTextChannel) await Program.Client.GetChannelAsync(id);
    public static bool TryFirst<T>(this IEnumerable<T> collection, Func<T, bool> condition, out T? first)
    {
        first = default;
        if (!collection.Any(condition)) return false;
        first = collection.First(condition);
        return true;
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

    public enum DynamicTimestampFormat
    {
        ShortTime,
        LongTime,
        ShortDate,
        LongDate,
        ShortDateTime,
        LongDateTime,
        RelativeTime
    }

    public static char FormatToLetter(this DynamicTimestampFormat format)
    {
        return format switch
        {
            DynamicTimestampFormat.ShortTime => 't',
            DynamicTimestampFormat.LongTime => 'T',
            DynamicTimestampFormat.ShortDate => 'd',
            DynamicTimestampFormat.LongDate => 'D',
            DynamicTimestampFormat.ShortDateTime => 'f',
            DynamicTimestampFormat.LongDateTime => 'F',
            DynamicTimestampFormat.RelativeTime => 'R',
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
    public static string ToDynamicTimestamp(this DateTimeOffset dto, DynamicTimestampFormat format = DynamicTimestampFormat.ShortDateTime)
    {
        return $"<t:{dto.ToUnixTimeSeconds()}:{format.FormatToLetter()}>";
    }
    public static Discord.Color Argb32ToDiscordColor(this Argb32 color) => new(color.R, color.G, color.B);
    public static Argb32 DiscordColorToArgb32(this Discord.Color color) => new(color.R, color.G, color.B, 255);
    
    public static IEnumerable<(TInfoType Member, IEnumerable<TAttribute> Attributes)>
        GetAllMembersWithAttribute<TInfoType, TAttribute>() where TInfoType  : MemberInfo 
                                                            where TAttribute : Attribute
    {
        MemberTypes memberType = MemberTypes.All;

        if (typeof(TInfoType).IsAssignableTo(typeof(FieldInfo)))
            memberType = MemberTypes.Field;
        else if (typeof(TInfoType).IsAssignableTo(typeof(MethodInfo)))
            memberType = MemberTypes.Method;
        else if (typeof(TInfoType).IsAssignableTo(typeof(PropertyInfo)))
            memberType = MemberTypes.Property;
        else if (typeof(TInfoType).IsAssignableTo(typeof(ConstructorInfo)))
            memberType = MemberTypes.Constructor;
        
        return GetAllMembersWithAttribute<TAttribute>(memberType)
            .Select<(MemberInfo Member, IEnumerable<TAttribute> Attributes), (TInfoType, IEnumerable<TAttribute>)>
                (x => ((TInfoType) x.Member, x.Attributes));
    }
    
    public static IEnumerable<(MemberInfo Member, IEnumerable<TAttribute> Attributes)>
        GetAllMembersWithAttribute<TAttribute>(MemberTypes filter = MemberTypes.All, Type? inType = null) where TAttribute : Attribute
    {
        if (inType is { })
            return inType.GetMembers(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.GetCustomAttributes<TAttribute>(false).Any()
                            && x.MemberType.HasFlag(filter))
                .Select(x => (x, x.GetCustomAttributes<TAttribute>(false)));
        
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(x => x.GetMembers(BindingFlags.Public | BindingFlags.Static))
            .Where(x => x.GetCustomAttributes<TAttribute>(false).Any()
                && x.MemberType.HasFlag(filter))
            .Select(x => (x, x.GetCustomAttributes<TAttribute>(false)));
    }
    
    public static T GetRequiredValue<T>(this SocketSlashCommand cmd, string optionName)
    {
        object value = cmd.Data.Options.First(x => x.Name == optionName).Value!;
        return (T) (value is long ? Convert.ToInt32(value) : value);
    }
    public static T GetOptionalValue<T>(this SocketSlashCommand cmd, string optionName, T defaultValue)
    {
        if (cmd.Data.Options.All(x => x.Name != optionName)) 
            return defaultValue;
        
        object value = cmd.Data.Options.First(x => x.Name == optionName).Value!;
        return (T) (value is long ? Convert.ToInt32(value) : value);
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

    public static T RandomChoice<T>(this IEnumerable<T> collection) =>
        collection.ToArray()[Rando.Int(0, collection.Count() - 1)];

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
        using var client = new HttpClient();
        
        return Regex.Match(
            client.GetStringAsync("https://randomword.com/").Result,
            "(?<=<div id=\"random_word\">).+(?=</div>)").Value;
    }
    public static SocketGuild GetGuild(this ISocketMessageChannel channel) => Program.Client.Guilds.First(x => x.Channels.Any(y => y.Id == channel.Id));
}