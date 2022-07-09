using Microsoft.EntityFrameworkCore;
using TLCBot2.Logging;
using TLCBot2.Utilities;

namespace TLCBot2.Types;

[PrimaryKey(nameof(ID))]
public class LogEntry
{
    public LogEntry(Log.Importance importance, string message, params string[] tags)
    {
        Tags = tags.Select(x => x.ToUpper().Replace(' ', '_')).ToJson();
        Message = message;
        Importance = (int) importance;
    }

    public int ID { get; set; }
    public string Tags { get; set; }
    public string Message { get; set; }
    public int Importance { get; set; }

    public void Deconstruct(out Log.Importance importance, out string message, out string[] tags)
    {
        importance = (Log.Importance) Importance;
        message = Message;
        tags = Tags.FromJson<string[]>()!;
    }
}