using System.Reflection;
using TLCBot2.Utilities;
using Timer = System.Timers.Timer;

namespace TLCBot2.Attributes;

public class TimedEventAttribute : Attribute
{
    public static Dictionary<double, Timer> Timers = new();

    public TimedEventAttribute(double millisecondInterval)
    {
        MillisecondInterval = millisecondInterval;
    }

    static TimedEventAttribute()
    {
        var methods = Helper.GetAllMembersWithAttribute<MethodInfo, TimedEventAttribute>()
            .Where(x => !x.Member.GetParameters().Any()
                && x.Member.IsStatic)
            .ToArray();

        foreach ((MethodInfo method, IEnumerable<TimedEventAttribute> attributes) in methods)
        {
            foreach (var interval in attributes.Select(x => x.MillisecondInterval))
            {
                if (!Timers.ContainsKey(interval))
                {
                    Timers.Add(interval, new Timer(interval));
                }
                
                Timers[interval].Elapsed += (_, _) => Task.Run(() => method.Invoke(null, null));
            }
        }

        foreach (var timer in Timers.Values)
        {
            timer.Start();
        }
    }
    
    public double MillisecondInterval { get; }
}