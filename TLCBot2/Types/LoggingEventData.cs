using System.Text.RegularExpressions;
using TLCBot2.Core;
using TLCBot2.Logging;
using TLCBot2.Utilities;

namespace TLCBot2.Types;

public record LoggingEventData(Log.Importance Importance, string[] Tags)
{
    public static readonly string Path = $"{Program.FileAssetsPath}/events/event_tags.json";
    public static readonly Dictionary<string,LoggingEventData> All = 
        Regex.Replace(File.ReadAllText(Path),
            @" *""\$schema"":\s*"".*"",?\s*\n", "")
        .FromJson<Dictionary<string, LoggingEventData>>()!;
}