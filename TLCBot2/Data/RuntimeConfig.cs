using System.Reflection;
using TLCBot2.Attributes;
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
    
    [PreInitialize]
    public static async Task LoadConfig()
    {
        if (!File.Exists(Path))
            await Task.Run(SaveConfig);
        foreach ((string name, object? fetchedValue) in (await Task.Run(() => File.ReadAllText(Path))).FromJson<RuntimeConfigFieldEntry[]>()!)
        {
            var field = typeof(RuntimeConfig).GetField(name)!;

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
    
    [RuntimeConfigField(ShortName = "fdch", DisplayValue = $"<#{RuntimeConfigFieldAttribute.ReplacementString}> | {RuntimeConfigFieldAttribute.ReplacementString}")]
    public static ulong FileDumpChannelId;
    
    [RuntimeConfigField(ShortName = "jdch", DisplayValue = $"<#{RuntimeConfigFieldAttribute.ReplacementString}> | {RuntimeConfigFieldAttribute.ReplacementString}")]
    public static ulong JsonDataChannelId;
}