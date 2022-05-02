using System.Text.RegularExpressions;
using Discord.WebSocket;
using MoreLinq;
using MoreLinq.Extensions;
using System.Data.SQLite;

namespace TLCBot2.Core;

public static class RuntimeConfig
{
    public static string ConfigPath => $"{Program.FileAssetsPath}/config.txt";
    public static string[] GetRuntimeProps() => typeof(RuntimeConfig).GetProperties()
        .Where(x => x.Name != "ConfigPath")
        .Select(x => x.Name)
        .ToArray();

    private static class Get
    {
        public static SocketGuild ParseGuild(string val) => 
            Program.Client
                .GetGuild(ulong.Parse(val));

        public static SocketTextChannel ParseTextChannel(string val) => 
            (SocketTextChannel)ParseChannel(val);
        public static SocketGuildChannel ParseChannel(string val) => 
            (SocketGuildChannel)Program.Client.GetChannel(ulong.Parse(val));
        public static SocketRole ParseRole(string val)
        {
            string[] split = val.Split('/');
            return Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetRole(ulong.Parse(split[1]));
        }
        public static SocketGuild Guild(string name) => 
            ParseGuild(UnsafeGetSetting<string>(name)!);
        public static SocketTextChannel TextChannel(string name) => 
            ParseTextChannel(UnsafeGetSetting<string>(name)!);
        public static SocketGuildChannel Channel(string name) => 
            ParseChannel(UnsafeGetSetting<string>(name)!);
        public static SocketRole Role(string name) =>
            ParseRole(UnsafeGetSetting<string>(name)!);
        public static T[] Array<T>(string name, Func<string, T> parseType)
        {
            return UnsafeGetSetting<string>(name)!
                .Split(',')
                .Select(parseType).ToArray();
        }
    }
    private static class Set
    {
        public static string ParseGuild(SocketGuild val) => val.Id.ToString();
        public static string ParseTextChannel(SocketTextChannel val) => ParseChannel(val);
        public static string ParseChannel(SocketGuildChannel val) => val.Id.ToString();
        public static string ParseRole(SocketRole val) => $"{val.Guild}/{val.Id}";
        public static void Guild(string name, SocketGuild newValue) => 
            SetSetting(name, ParseGuild(newValue));
        public static void TextChannel(string name, SocketTextChannel newValue) => 
            SetSetting(name, ParseTextChannel(newValue));
        public static void Channel(string name, SocketGuildChannel newValue) => 
            SetSetting(name, ParseChannel(newValue));
        public static void Role(string name, SocketRole newValue) =>
            SetSetting(name, ParseRole(newValue));
        public static void Array<T>(string name, IEnumerable<T> newValue, Func<T, string> parseType)
        {
            SetSetting(name, string.Join(",", newValue.Select(parseType)));
        }
    }

    public static SocketTextChannel[] WhitelistedStarboardChannels
    {
        get => Get.Array("WhitelistedStarboardChannels", Get.ParseTextChannel);
        set => Set.Array("WhitelistedStarboardChannels", value, Set.ParseTextChannel);
    }
    public static SocketTextChannel StarboardChannel
    {
        get => Get.TextChannel("StarboardChannel");
        set => Set.TextChannel("StarboardChannel", value);
    }

    public static SocketTextChannel BotReportsChannel
    {
        get => Get.TextChannel("BotReportsChannel");
        set => Set.TextChannel("BotReportsChannel", value);
    }
    
    public static SocketTextChannel VentingChannel
    {
        get => Get.TextChannel("VentingChannel");
        set => Set.TextChannel("VentingChannel", value);
    }
    public static SocketRole AdminRole
    {
        get => Get.Role("AdminRole");
        set => Set.Role("AdminRole", value);
    }
    public static SocketRole SecretsRole
    {
        get => Get.Role("SecretsRole");
        set => Set.Role("SecretsRole", value);
    }
    public static SocketRole QOTDRole
    {
        get => Get.Role("QOTDRole");
        set => Set.Role("QOTDRole", value);
    }
    public static SocketTextChannel QOTDChannel
    {
        get => Get.TextChannel("QOTDChannel");
        set => Set.TextChannel("QOTDChannel", value);
    }
    public static SocketTextChannel DashboardChannel
    {
        get => Get.TextChannel("DashboardChannel");
        set => Set.TextChannel("DashboardChannel", value);
    }
    public static SocketTextChannel ModDashboardChannel
    {
        get => Get.TextChannel("ModDashboardChannel");
        set => Set.TextChannel("ModDashboardChannel", value);
    }
    public static SocketTextChannel CritiqueMyWorkChannel
    {
        get => Get.TextChannel("CritiqueMyWorkChannel");
        set => Set.TextChannel("CritiqueMyWorkChannel", value);
    }
    public static SocketTextChannel ServerSuggestionsChannel
    {
        get => Get.TextChannel("ServerSuggestionsChannel");
        set => Set.TextChannel("ServerSuggestionsChannel", value);
    }
    public static SocketTextChannel TLCBetaCommandLine
    {
        get => Get.TextChannel("TLCBetaCommandLine");
        set => Set.TextChannel("TLCBetaCommandLine", value);
    }
    public static SocketGuild FocusServer
    {
        get => Get.Guild("FocusServer");
        set => Set.Guild("FocusServer", value);
    }
    public static SocketTextChannel CookieLogChannel
    {
        get => Get.TextChannel("CookieLogChannel");
        set => Set.TextChannel("CookieLogChannel", value);
    }
    public static SocketTextChannel DefaultFileDump
    {
        get => Get.TextChannel("DefaultFileDump");
        set => Set.TextChannel("DefaultFileDump", value);
    }
    public static SocketTextChannel FeedbackReceptionChannel
    {
        get => Get.TextChannel("FeedbackReceptionChannel");
        set => Set.TextChannel("FeedbackReceptionChannel", value);
    }
    public static SocketTextChannel TLCLogs
    {
        get => Get.TextChannel("TLCLogs");
        set => Set.TextChannel("TLCLogs", value);
    }
    public static SocketTextChannel UselessLogs
    {
        get => Get.TextChannel("UselessLogs");
        set => Set.TextChannel("UselessLogs", value);
    }
    public static SocketTextChannel DoodleOnlyChannel
    {
        get => Get.TextChannel("DoodleOnlyChannel");
        set => Set.TextChannel("DoodleOnlyChannel", value);
    }
    public static SocketGuildChannel StatMembersVC
    {
        get => Get.Channel("StatMembersVC");
        set => Set.Channel("StatMembersVC", value);
    }
    public static SocketGuildChannel StatDaysActiveVC
    {
        get => Get.Channel("StatDaysActiveVC");
        set => Set.Channel("StatDaysActiveVC", value);
    }
    public static SocketTextChannel GeneralChat
    {
        get => Get.TextChannel("GeneralChat");
        set => Set.TextChannel("GeneralChat", value);
    }
    public static void Initialize()
    {
        string[] props = GetRuntimeProps();
        string[] lines = File.ReadAllLines(ConfigPath);
        File.WriteAllLines(ConfigPath, props.Select(prop =>
        {
            bool Condition(string x) => x.StartsWith(prop);
            return lines.Any(Condition) 
                ? lines.First(Condition)
                : $"{prop}:null";
        }));
    }
    public static bool GetSetting<T>(string name, out T val) where T : IConvertible
    {
        val = default!;
        bool Condition(string x) => x.StartsWith($"{name}:");
        
        string[] lines = File.ReadAllLines(ConfigPath);
        if (!lines.Any(Condition)) return false;
        
        string stringVal = lines.Single(Condition);
        if (stringVal == "null") return false;

        val = (T) Convert.ChangeType(Regex.Match(stringVal, $"(?<={name}:).+").Value, typeof(T));
        return true;
    }
    public static T? UnsafeGetSetting<T>(string name) where T : IConvertible
    {
        string val = File.ReadAllLines(ConfigPath)
            .Single(x => x.StartsWith($"{name}:"));
        if (val != "null")
            return (T) Convert.ChangeType(Regex.Match(val, $"(?<={name}:).+").Value, typeof(T));
        return default;
    }

    public static bool SetSetting(string name, string newVal, out string propertyName)
    {
        propertyName = null!;
        string[] lines = File.ReadAllLines(ConfigPath);
        bool Condition(string x) => x.ToLower().StartsWith(name.ToLower());
        
        if (!lines.Any(Condition)) return false;

        string[] props = GetRuntimeProps();
        
        string lineToChange = lines.First(Condition);
        string propVal = props.First(x => string.Equals(x, name, StringComparison.CurrentCultureIgnoreCase));
        File.WriteAllLines(ConfigPath, lines.Select(x => x == lineToChange ? $"{propVal}:{newVal}" : x));
        
        propertyName = propVal;
        return true;
    }
    public static bool SetSetting(string name, string newVal)
    {
        return SetSetting(name, newVal, out _);
    }
}