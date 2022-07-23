using TLCBot2.Logging;

namespace TLCBot2.Types;

public record LogNotificationSettings(
    List<string[]> AppliedTagSets,
    List<string> AppliedEvents,
    List<Log.Importance> AppliedImportances);