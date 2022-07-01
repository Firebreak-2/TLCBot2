using System.ComponentModel;
using System.Reflection;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.CommandLine;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Data;

public static class RuntimeConfig
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
    
    [PreInitialize]
    public static async Task LoadConfig()
    {
        if (!File.Exists(Path))
            await Task.Run(SaveConfig);
        foreach ((string name, object? fetchedValue) in (await Task.Run(() => File.ReadAllText(Path))).FromJson<RuntimeConfigFieldEntry[]>()!)
        {
            if (typeof(RuntimeConfig).GetField(name) is not { } field)
                continue;
            
            object? convertedValue = Convert.ChangeType(fetchedValue, field.FieldType);
            
            field.SetValue(null, convertedValue);
        }
    }

    // runs every minute
    [TimedEvent(1000 * 60)]
    public static Task SaveConfig()
    {
        RuntimeConfigFieldEntry[] fields = Fields
            .Select(x => new RuntimeConfigFieldEntry(x.Field.Name, x.Field.GetValue(null)))
            .ToArray();
        File.WriteAllText(Path, fields.ToJson());

        return Task.CompletedTask;
    }

    public static ulong TerminalChannelId { get; } = ulong.Parse(Environment.GetEnvironmentVariable("TERMINAL") 
                                                                 ?? throw new Exception("Terminal channel id not provided"));

    public static SocketGuild? FocusServer;
    
    [RuntimeConfigField(ShortName = "dump", DisplayValue = ChannelDisplayValue)]
    public static ulong FileDumpChannelId;
    
    [RuntimeConfigField(ShortName = "json", DisplayValue = ChannelDisplayValue)]
    public static ulong JsonDataChannelId;

    [RuntimeConfigField(ShortName = "logs", DisplayValue = ChannelDisplayValue)]
    public static ulong ServerLogsChannelId;

    private const string ChannelDisplayValue =
        $"<#{RuntimeConfigFieldAttribute.ReplacementString}> | {RuntimeConfigFieldAttribute.ReplacementString}";
}