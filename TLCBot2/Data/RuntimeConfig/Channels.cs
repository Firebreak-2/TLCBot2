using TLCBot2.Attributes;

namespace TLCBot2.Data.RuntimeConfig;

public static partial class RuntimeConfig
{
    [RuntimeConfigField(ShortName = "dump", DisplayValue = ChannelDisplayValue)]
    public static ulong FileDumpChannelId;
    
    [RuntimeConfigField(ShortName = "json", DisplayValue = ChannelDisplayValue)]
    public static ulong JsonDataChannelId;

    [RuntimeConfigField(ShortName = "logs", DisplayValue = ChannelDisplayValue)]
    public static ulong ServerLogsChannelId;

    [RuntimeConfigField(ShortName = "feedback", DisplayValue = ChannelDisplayValue)]
    public static ulong UserFeedbackChannelId;

    [RuntimeConfigField(ShortName = "reports", DisplayValue = ChannelDisplayValue)]
    public static ulong BotReportsChannelId;

    private const string ChannelDisplayValue =
        $"<#{RuntimeConfigFieldAttribute.ReplacementString}> | {RuntimeConfigFieldAttribute.ReplacementString}";

}