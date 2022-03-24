using System.Text.RegularExpressions;
using Discord.WebSocket;
using MoreLinq;
using MoreLinq.Extensions;

namespace TLCBot2.Core;

public static class RuntimeConfig
{
    public static string ConfigPath => $"{Program.FileAssetsPath}/config.txt";
    public static string[] GetRuntimeProps() => typeof(RuntimeConfig).GetProperties()
        .Where(x => x.Name != "ConfigPath")
        .Select(x => x.Name)
        .ToArray();
    public static SocketGuildChannel[] WhitelistedStarboardChannels
    {
        get => UnsafeGetSetting<string>("WhitelistedStarboardChannels")!
            .Split(',')
            .Select(x => Program.Client.GetGuild(ulong.Parse(x.Split('/')[0]))
                .GetChannel(ulong.Parse(x.Split('/')[1]))).ToArray();
        set => SetSetting("WhitelistedStarboardChannels", string.Join(",", value.Select(x => $"{x.Guild.Id}/{x.Id}")));
    }
    public static SocketTextChannel StarboardChannel
    {
        get
        {
            string x = UnsafeGetSetting<string>("StarboardChannel")!;
            string[] split = x.Split('/');
            return (SocketTextChannel)Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetChannel(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("StarboardChannel", $"{value.Guild.Id}/{value.Id}");
    }
    public static SocketRole AdminRole
    {
        get
        {
            string x = UnsafeGetSetting<string>("AdminRole")!;
            string[] split = x.Split('/');
            return Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetRole(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("AdminRole", $"{value.Guild.Id}/{value.Id}");
    }
    public static SocketRole QOTDRole
    {
        get
        {
            string x = UnsafeGetSetting<string>("QOTDRole")!;
            string[] split = x.Split('/');
            return Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetRole(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("QOTDRole", $"{value.Guild.Id}/{value.Id}");
    }
    public static SocketTextChannel QOTDChannel
    {
        get
        {
            string x = UnsafeGetSetting<string>("QOTDChannel")!;
            return (SocketTextChannel)Program.Client.GetChannel(ulong.Parse(x));
        }
        set => SetSetting("QOTDChannel", $"{value.Id}");
    }
    public static SocketTextChannel ServerSuggestionsChannel
    {
        get
        {
            string x = UnsafeGetSetting<string>("ServerSuggestionsChannel")!;
            return (SocketTextChannel)Program.Client.GetChannel(ulong.Parse(x));
        }
        set => SetSetting("ServerSuggestionsChannel", $"{value.Id}");
    }
    public static SocketTextChannel TLCBetaCommandLine
    {
        get
        {
            string x = UnsafeGetSetting<string>("TLCBetaCommandLine")!;
            return (SocketTextChannel)Program.Client.GetChannel(ulong.Parse(x));
        }
        set => SetSetting("TLCBetaCommandLine", $"{value.Id}");
    }
    public static SocketTextChannel CookieLogChannel
    {
        get
        {
            string x = UnsafeGetSetting<string>("CookieLogChannel")!;
            string[] split = x.Split('/');
            return (SocketTextChannel)Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetChannel(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("CookieLogChannel", $"{value.Guild.Id}/{value.Id}");
    }
    public static SocketTextChannel DefaultFileDump
    {
        get
        {
            string x = UnsafeGetSetting<string>("DefaultFileDump")!;
            string[] split = x.Split('/');
            return (SocketTextChannel)Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetChannel(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("DefaultFileDump", $"{value.Guild.Id}/{value.Id}");
    }
    public static SocketTextChannel FeedbackReceptionChannel
    {
        get
        {
            string x = UnsafeGetSetting<string>("FeedbackReceptionChannel")!;
            return (SocketTextChannel)Program.Client.GetChannel(ulong.Parse(x));
        }
        set => SetSetting("FeedbackReceptionChannel", $"{value.Id}");
    }
    public static SocketTextChannel CommandExecutionLog
    {
        get
        {
            string x = UnsafeGetSetting<string>("CommandExecutionLog")!;
            return (SocketTextChannel)Program.Client.GetChannel(ulong.Parse(x));
        }
        set => SetSetting("CommandExecutionLog", $"{value.Id}");
    }
    public static SocketTextChannel DoodleOnlyChannel
    {
        get
        {
            string x = UnsafeGetSetting<string>("DoodleOnlyChannel")!;
            string[] split = x.Split('/');
            return (SocketTextChannel)Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetChannel(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("DoodleOnlyChannel", $"{value.Guild.Id}/{value.Id}");
    }
    public static SocketGuildChannel StatMembersVC
    {
        get
        {
            string x = UnsafeGetSetting<string>("StatMembersVC")!;
            string[] split = x.Split('/');
            return Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetChannel(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("StatMembersVC", $"{value.Guild.Id}/{value.Id}");
    }
    public static SocketGuildChannel StatDaysActiveVC
    {
        get
        {
            string x = UnsafeGetSetting<string>("StatDaysActiveVC")!;
            string[] split = x.Split('/');
            return Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetChannel(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("StatDaysActiveVC", $"{value.Guild.Id}/{value.Id}");
    }
    public static SocketTextChannel GeneralChat
    {
        get
        {
            string x = UnsafeGetSetting<string>("GeneralChat")!;
            string[] split = x.Split('/');
            return (SocketTextChannel)Program.Client.GetGuild(ulong.Parse(split[0]))
                .GetChannel(ulong.Parse(x.Split('/')[1]));
        }
        set => SetSetting("GeneralChat", $"{value.Guild.Id}/{value.Id}");
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