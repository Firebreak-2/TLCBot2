using TLCBot2.Types;

namespace TLCBot2.Logging.LogConfig;

public static class ChannelLogFilter
{
    public static bool ShouldLog(LogEntry entry)
    {
        return true;
    }
}