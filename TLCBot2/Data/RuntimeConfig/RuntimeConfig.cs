using System.Reflection;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.CommandLine;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Data.RuntimeConfig;

public static partial class RuntimeConfig
{
    public static string Path => $"{Program.FileAssetsPath}/config.json";
    public static readonly (FieldInfo Field, RuntimeConfigFieldAttribute Attribute)[] Fields;

    static RuntimeConfig()
    {
        Fields = Helper.GetAllMembersWithAttribute<RuntimeConfigFieldAttribute>(MemberTypes.Field, typeof(RuntimeConfig))
            .Select(x => ((FieldInfo) x.Member, x.Attributes.First()))
            .ToArray();
    }

    [Initialize(Priority = 998)]
    public static Task Initialize()
    {
        FocusServer = ChannelTerminal.Channel.GetGuild();

        return Task.CompletedTask;
    }
    
    [Initialize(Priority = 997)]
    public static async Task LoadConfig()
    {
        if (!File.Exists(Path))
            await Task.Run(SaveConfig);
        foreach ((string name, string? fetchedValue) in (await Task.Run(() => File.ReadAllText(Path))).FromJson<Dictionary<string, string?>>()!)
        {
            if (typeof(RuntimeConfig).GetField(name) is not { } field)
                continue;
            
            object? convertedValue = Deserialize(fetchedValue, field.FieldType);
            
            field.SetValue(null, convertedValue);
        }

        await Task.Run(SaveConfig);
    }

    // runs every minute
    [TimedEvent(1000 * 60)]
    public static Task SaveConfig()
    {
        Dictionary<string, string?> fields = Fields
            .ToDictionary(x => x.Field.Name, x => Serialize(x.Field.GetValue(null)));
        File.WriteAllText(Path, fields.ToJson());

        return Task.CompletedTask;
    }
    
    private static string? Serialize(object? obj) => 
        Helper.ConvertToString(obj);

    private static object? Deserialize(string? strVal, Type type) => 
        Helper.ConvertFromString(strVal, type);

    public static ulong TerminalChannelId { get; } = ulong.Parse(Environment.GetEnvironmentVariable("TERMINAL") 
                                                                 ?? throw new Exception("Terminal channel id not provided"));

    public static SocketGuild? FocusServer;
}