namespace TLCBot2.Types;

public record MessageReminder(string Reminder)
{
    public string Reminder = Reminder;
    public bool FailIfConsecutive;
    public TimeSpan Interval;
}