using System.Security.AccessControl;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using TLCBot2.Logging;
using TLCBot2.Utilities;

namespace TLCBot2.Types;

[PrimaryKey(nameof(ID))]
public class LogEntry
{
    public long TimeStamp { get; set; }
    public int ID { get; set; }
    /// <summary>
    /// A JSON string representing a <see cref="string"/>[]
    /// </summary>
    public int Importance { get; set; }
    public string EventName { get; set; }
    public string Message { get; set; }
    public string Tags { get; set; }
    public string Data { get; set; }

    public LogEntry(string eventName, Log.Importance importance, string message, IEnumerable<string> tags, string data) 
        : this(DateTimeOffset.Now.ToUnixTimeSeconds(), importance, message, eventName, tags, data)
    { }
    public LogEntry(long timeStamp, int importance, string message, string eventName, string tags, string data)
    {
        TimeStamp = timeStamp;
        Importance = importance;
        Message = message;
        Tags = tags;
        EventName = eventName;
        Data = data;
    }
    public LogEntry(long timeStamp, Log.Importance importance, string message, string eventName, IEnumerable<string> tags, string data)
        : this(timeStamp, (int) importance, message, eventName, tags.Select(x => x.ToUpper().Replace(' ', '_')).ToArray().ToJson(), data)
    { }

    public UsableLogEntry ToUsable() => new()
    {
        OccurrenceTimeEpoch = TimeStamp,
        EventName = EventName,
        Importance = (Log.Importance) Importance,
        Message = Message,
        Tags = Tags.FromJson<string[]>()!,
        Data = Data.FromJson<Dictionary<string, object>>()!
    };
    
    public record UsableLogEntry
    {
        public long OccurrenceTimeEpoch { get; init; }
        public string EventName { get; init; }
        public Log.Importance Importance { get; init; }
        public int ImportanceValue => (int) this.Importance;
        public string Message { get; init; }
        public string[] Tags { get; init; }

        public Dictionary<string, string> VariableTags =>
            Tags.Where(x => x.Contains('='))
                .Select(x => x.Split('='))
                .ToDictionary(x => x[0], x => x[1]);
        public Dictionary<string, object> Data { get; init; }
    }
}
