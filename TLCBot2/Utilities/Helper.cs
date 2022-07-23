using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.Utilities;

public static partial class Helper
{
    public static FontCollection TLCFonts = new();
    public static FontFamily ArialFont;
    
    [PreInitialize]
    public static Task PreInitialize()
    {
        TLCFonts.Add(Program.FileAssetsPath + "/arial.ttf");
        ArialFont = TLCFonts.Get("Arial");

        return Task.CompletedTask;
    }

    public static bool CaselessEquals(this string str, string str2) => 
        string.Equals(str, str2, StringComparison.CurrentCultureIgnoreCase);

    public static readonly Regex MentionDeFormatRegex = new(@"<(#|@[!&]?)(\d+)>", RegexOptions.Compiled);
    private static Random _rand = new();
    private static DataTable _dataTable = new();
    public static string Compute(string input) => _dataTable.Compute(input, null).ToString() ?? "null";
    public static string GoBackDirectory(string path)
    {
        return Regex.Replace(path, @"/[^/]+$", "");
    }
    
    public static float GetHue(this Discord.Color color)
    {
        int red = color.R;
        int green = color.G;
        int blue = color.B;
        
        float min = Math.Min(Math.Min(red, green), blue);
        float max = Math.Max(Math.Max(red, green), blue);

        if (Math.Abs(min - max) < 0.05f)
            return 0;

        float hue = Math.Abs(max - red) < 0.05f
            ? (green - blue) / (max - min)
            : Math.Abs(max - green) < 0.05f
                ? 2f + (blue - red) / (max - min)
                : 4f + (red - green) / (max - min);

        hue *= 60;
        
        if (hue < 0) 
            hue += 360;

        return hue;
    }

    public static string GetHyperLink(this string text, string link) =>
        $"[{text}]({link})";
    public static string GetJumpHyperLink(this IMessage msg, string message = "Go To Message") =>
        message.GetHyperLink(msg.GetJumpUrl());
    public static string GetFileNameFromPath(string path)
    {
        return Regex.Match(path, @"/[^/]+$").Value;
    }

    public static string GetEmoteID(this IEmote emote)
    {
        return emote is GuildEmote guildEmote 
            ? guildEmote.Id.ToString() 
            : emote.Name;
    }

    public static string GetEmoteFullName(this IEmote emote)
    {
        throw new NotImplementedException();
    }

    private static readonly string _ansiBlueBg = Ansi.Generate(null, Ansi.Background.Indigo);
    private static readonly string _ansiOrangeBg = Ansi.Generate(null, Ansi.Background.Orange);
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

    public static string GenerateRegexHighlightedCodeBlocks([RegexPattern] string pattern, string matchString, bool showPattern = true)
    {
        var paragraphRegex = new Regex($"(?:{pattern})+");
        string finalParagraphString = paragraphRegex.Replace(matchString, $"{_ansiBlueBg}$&{Ansi.Reset}");
        string finalRegexString = pattern;
        
        if (showPattern)
        {
            foreach ((Regex regex, string color) in _regexTokenizer)
            {
                finalRegexString = regex.Replace(finalRegexString, match => $"{color}{match.Value}{_ansiWhiteFg}");
            }
        }

        return $"{(showPattern ? $"```ansi\n{_ansiWhiteFg}{finalRegexString}\x1B[0m\n```\n" : "")}```ansi\n{finalParagraphString}\n```";
    }

    public static Dictionary<string, string> GetMessageDetails(this IMessage message)
    {
        return new Dictionary<string, object>
        {
            {"Author", message.Author.Id},
            {"Content", message.Content},
            {"Attachments", string.Join('\n', message.Attachments.Select(x => x.Url))},
            {"Reactions", string.Join('\n', message.Reactions.Select(x => $"{x.Key.Name}={x.Value.ReactionCount}"))}
        }.ToDictionary(x => x.Key, x => x.Value?.ToString() ?? "null");
    }
    public static Dictionary<string, string> GetChannelDetails(this IChannel channel, params string[] without)
    {
        var dict = new Dictionary<string, string>();

        switch (channel)
        {
            case SocketCategoryChannel c:
                dict.Add("Category Name", c.Name.ToUpper());
                dict.Add("Channels", string.Join('\n', c.Channels.Select(x => x.Name)));
                dict.Add("Created", c.CreatedAt.ToUnixTimeSeconds().ToString());
                break;
            case SocketThreadChannel c:
                dict.Add("Thread Name", c.Name);
                dict.Add("Parent Channel", c.ParentChannel.Name);
                dict.Add("Is NSFW", c.IsNsfw.ToString());
                dict.Add("Is Private", c.IsPrivateThread.ToString());
                dict.Add("Auto Archive Diration", c.AutoArchiveDuration.ToString());
                dict.Add("Created", c.CreatedAt.ToUnixTimeSeconds().ToString());
                break;
            case SocketVoiceChannel c:
                dict.Add("Channel Name", c.Name);
                dict.Add("Category", c.Category.Name.ToUpper());
                dict.Add("User Limit", c.UserLimit?.ToString() ?? "∞");
                dict.Add("Region", c.RTCRegion ?? "Auto");
                dict.Add("Is NSFW", c.IsNsfw.ToString());
                dict.Add("Created", c.CreatedAt.ToUnixTimeSeconds().ToString());
                break;
            case SocketTextChannel c:
                dict.Add("Channel Name", c.Name);
                dict.Add("Category", c.Category.Name.ToUpper());
                dict.Add("Topic", c.Topic is null or {Length: 0} ? "No Topic" : c.Topic);
                dict.Add("Is NSFW", c.IsNsfw.ToString());
                dict.Add("Created", c.CreatedAt.ToUnixTimeSeconds().ToString());
                break;
        }

        dict.Add("Type", channel.GetChannelType()?.ToString() ?? "null");
        dict.Add("Channel ID", channel.Id.ToString());
        
        return dict.Where(x => !without.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
    }

    public static string ToCodeBlock(this string code, string language = "")
    {
        return $"```{language}\n{code}\n```";
    }
    public static string ToJson(this object? obj, Formatting formatting = Formatting.Indented) =>
        JsonConvert.SerializeObject(obj, formatting);
    public static T? FromJson<T>(this string str) =>
        JsonConvert.DeserializeObject<T>(str);
    public static bool CheckStringIsLink(string link) =>
        Regex.IsMatch(link,
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");

    public static string SequentialFormat(this string str, params object[] arguments)
    {
        var regex = new Regex(@"(?<!\{)\{\}(?!\})");
        for (int i = 0; regex.IsMatch(str); i++)
        {
            if (i == arguments.Length)
                break;
            str = regex.Replace(str, arguments[i].ToString() ?? "", 1);
        }
        str = str.Replace("{{", "{").Replace("}}", "}");
        return str;
    }
    
    public static string MappedFormat(this string str, params (string Key, object Replacement)[] arguments)
    {
        return arguments.Aggregate(str, (current, argument) => 
            Regex.Replace(current, $@"(?<!\{{)\{{{argument.Key}\}}(?!\}})", 
                argument.Replacement.ToString() ?? ""))
            .Replace("{{", "{").Replace("}}", "}");
    }
    
    public static string SequentialMappedFormat(this string str, params (string Key, Stack<object> Replacement)[] arguments)
    {
        foreach ((string key, Stack<object> replacement) in arguments)
        {
            if (!replacement.TryPop(out object? currentReplacement)) 
                continue;
            
            str = Regex.Replace(str, $@"(?<!\{{)\{{{key}\}}(?!\}})", 
                currentReplacement.ToString() ?? "");
        }
        return str.Replace("{{", "{").Replace("}}", "}");
    }
    
    public static async Task<string> GetFileUrl(Stream stream, SocketTextChannel? channel = null, string text = "text")
    {
        channel ??= RuntimeConfig.FileDumpChannel;
        if (channel is null)
            throw new Exception("File dump channel not registered");
        
        var msg = await channel.SendFileAsync(stream, "ImageSend.png", text);
        return msg.Attachments.First().Url;
    }

    public static async Task<Image<Argb32>> ImageFromUrl(string url)
    {
        using var client = new HttpClient();
        byte[] data = await client.GetByteArrayAsync(new Uri(url));
        return Image.Load<Argb32>(data);
    }
    public static async Task DisableMessageComponentsAsync(IUserMessage message)
    {
        if (message.Author.Id != Program.Client.CurrentUser.Id) 
            return;
        
        await message.ModifyAsync(props =>
        {
            var componentBuilder = new ComponentBuilder();
            foreach (var actionRow in message.Components)
            {
                var row = new ActionRowBuilder();
                foreach (var component in ((ActionRowComponent) actionRow).Components)
                {
                    switch (component.Type)
                    {
                        case ComponentType.Button:
                            row.AddComponent(((ButtonComponent) component)
                                .ToBuilder()
                                .WithDisabled(true)
                                .WithStyle(ButtonStyle.Secondary).Build());
                            break;
                        case ComponentType.SelectMenu:
                            row.AddComponent(((SelectMenuComponent) component)
                                .ToBuilder()
                                .WithDisabled(true).Build());
                            break;
                        
                        default:
                            return;
                    }
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

    public static int AsInt(this bool boolean) => boolean ? 1 : 0;
    public static bool AsBool(this int num) => num == 1;

    /// <returns>The ID of the message that contains the json data</returns>
    public static async Task<ulong?> StoreJsonData(object data, ulong? editId = null)
    {
        string json = data is string str ? str : data.ToJson();
        string formattedJson = $"```json\n{json}\n```";
        
        if (RuntimeConfig.JsonDataChannel is not { } channel) 
            return null;
        
        if (editId is not { } id)
            return (await channel.SendMessageAsync(formattedJson)).Id;

        await channel.ModifyMessageAsync(id, props => props.Content = formattedJson);
        return editId;
    }

    public static async Task<string?> FetchJsonData(ulong messageId)
    {
        if (RuntimeConfig.JsonDataChannel is not { } channel) 
            return null;
        
        return ExtractCodeFromCodeBlock((await channel.GetMessageAsync(messageId)).Content)
            is ("json", var code) ? code : null;
    }

    public static async Task<T?> FetchJsonData<T>(ulong messageId) where T : class
    {
        return (await FetchJsonData(messageId))?.FromJson<T>();
    }
    
    private static readonly Regex _extractCodeRegex = new(@"```([a-z0-9]+)?\n?((?:.|[\n\r])*)```", RegexOptions.Compiled);
    
    public static (string? Language, string Code)? ExtractCodeFromCodeBlock(string codeBlock)
    {
        var match = _extractCodeRegex.Match(codeBlock);
        if (!match.Success)
            return null;
        string lang = match.Groups[1].Value;
        string code = match.Groups[2].Value;
        return (lang == "" ? null : lang, code);
    }

    public static string GenerateProgressBar(
        float current,
        float max,
        int characterCount = 20,
        char filled = '#',
        char empty = '-')
    {
        float fillPercentage = (max > 0 ? current / max : current) * characterCount;

        string whiteBar = $"{(fillPercentage > 0 ? new string(filled, (int)fillPercentage) : "")}";
        string blackBar = $"{new string(empty, (int)(characterCount - fillPercentage))}";
        
        string fullBar = $"{whiteBar}{blackBar}";
        fullBar = fullBar.Length == characterCount - 1 
            ? fullBar + empty
            : fullBar;
        
        return fullBar;
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

    public static TComponent GetSpecificComponent<TComponent>(this SocketUserMessage message, string customId) where TComponent : IMessageComponent
    {
        ComponentType type;

        if (typeof(TComponent) == typeof(ButtonComponent))
            type = ComponentType.Button;
        else if (typeof(TComponent) == typeof(SelectMenuComponent))
            type = ComponentType.SelectMenu;
        else throw new Exception("invalid message component type");
        
        return (TComponent) message.Components
            .First(x => x.Components
                .Any(y => y.CustomId == customId))
            .Components
            .First(x => x.Type == type 
                        && x.CustomId == customId);
    }
    public static async Task<IEnumerable<IUserMessage>> GetLatestMessages(this SocketTextChannel channel, int limit = 100) => (await channel
        .GetMessagesAsync(limit)
        .ToArrayAsync()).SelectMany(x => x)
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
        ShortTime = 't',
        LongTime = 'T',
        ShortDate = 'd',
        LongDate = 'D',
        ShortDateTime = 'f',
        LongDateTime = 'F',
        RelativeTime = 'R',
    }
    
    public static FieldInfo[] GetConstants(this Type type)
    {
        // https://stackoverflow.com/a/10261848
        
        ArrayList constants = new ArrayList();

        FieldInfo[] fieldInfos = type.GetFields(

            // Gets all public and static fields
            BindingFlags.Public | BindingFlags.Static | 

            // This tells it to get the fields from all base types as well
            BindingFlags.FlattenHierarchy);

        // Go through the list and only pick out the constants
        foreach (FieldInfo fi in fieldInfos)
            // IsLiteral determines if its value is written at 
            //   compile time and not changeable
            // IsInitOnly determines if the field can be set 
            //   in the body of the constructor
            // for C# a field which is readonly keyword would have both true 
            //   but a const field would have only IsLiteral equal to true
            if (fi.IsLiteral && !fi.IsInitOnly)
                constants.Add(fi);           

        // Return an array of FieldInfos
        return (FieldInfo[])constants.ToArray(typeof(FieldInfo));
    }

    public static char FormatToLetter(this DynamicTimestampFormat format) => (char) (int) format;

    public static string ToDynamicTimestamp(
        this DateTimeOffset dto, 
        DynamicTimestampFormat format = DynamicTimestampFormat.ShortDateTime)
    {
        return $"<t:{dto.ToUnixTimeSeconds()}:{format.FormatToLetter()}>";
    }

    private static readonly Regex _dtoFromTimeRegex = new(@"<t:(\d+)(?::([tTdDfFR]))?>", RegexOptions.Compiled);  
    public static (DateTimeOffset DateTimeOffset, DynamicTimestampFormat Format)? DateTimeOffsetFromDynamicTimestamp(string str)
    {
        var match = _dtoFromTimeRegex.Match(str);
        if (!match.Success)
            return null;
        
        DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(match.Groups[1].Value.To<long>());
        DynamicTimestampFormat format = match.Groups[2].Value is {Length: 1} c
            ? (DynamicTimestampFormat) c[0]
            : DynamicTimestampFormat.ShortDateTime;
        
        return (offset, format);
    }

    public static string EnsureString(this string? str, string defaultValue = "_") =>
        string.IsNullOrEmpty(str) 
            ? defaultValue 
            : str;
    public static Discord.Color Argb32ToDiscordColor(this Argb32 color) => new(color.R, color.G, color.B);
    public static Argb32 DiscordColorToArgb32(this Discord.Color color) => new(color.R, color.G, color.B, 255);

    public static string GetJumpURL(this SocketGuildEvent guildEvent) =>
        $"https://discord.com/events/{guildEvent.Guild.Id}/{guildEvent.Id}";
    
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
    public static SocketGuild GetGuild(this ISocketMessageChannel channel) => Program.Client.Guilds.First(x => x.Channels.Any(y => y.Id == channel.Id));
}