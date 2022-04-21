using TLCBot2.Utilities;
using Timer = System.Timers.Timer;

namespace TLCBot2.Listeners.TimedEvents;

public static class TheFlowOfTime
{
    public static void Initialize()
    {
        // SixHourCLock.Elapsed += (_, _) => Helper.RestartProgram();
        // HourCLock.Elapsed += (_, _) => ServerStatsListener.UpdateDaysSinceOpen();
    }

    public static readonly Timer MinuteCLock = new(5000)
    {
        Enabled = true,
        AutoReset = true
    };

    public static readonly Timer HourCLock = new(_hourTickSpeed)
    {
        Enabled = true,
        AutoReset = true
    };

    public static readonly Timer SixHourCLock = new(_sixHourTickSpeed)
    {
        Enabled = true,
        AutoReset = true
    };

    public static readonly Timer DailyCLock = new(_dailyTickSpeed)
    {
        Enabled = true,
        AutoReset = true
    };
    private const double _secondTickSpeed = 1000;                  // 1000 milliseconds = 1 second
    private const double _minuteTickSpeed = _secondTickSpeed * 60; // 60 seconds = 1 minute
    private const double _hourTickSpeed = _minuteTickSpeed * 60;   // 60 minutes = 1 hour
    private const double _sixHourTickSpeed = _hourTickSpeed * 6;
    private const double _dailyTickSpeed = _hourTickSpeed * 24;    // 24 hours = 1 day
}