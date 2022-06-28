using Discord;
using TLCBot2.Core;

namespace TLCBot2.Utilities;

public static partial class Helper
{
    public static object ConvertFromString(string strVal, Type type)
    {
        // i would have used a switch statement
        // for this but unfortunately doing
        // typeof(T) does not work in switch cases
        // because it says it's not a compile-time
        // constant. so here we are stuck with this
        // abomination.
        
        if (type.IsAssignableTo(typeof(Enum)))
        {
            var values = Enum.GetValues(type).Cast<Enum>();
            if (values.TryFirst(x => string.Equals($"{x}", strVal, StringComparison.CurrentCultureIgnoreCase),
                    out var val))
            {
                return val!;
            }
            else
            {
                throw new Exception($"Enum type [{type.Name}] does not have [{strVal}] as a valid value");
            }
        }
        else if (type == typeof(DateTimeOffset))
        {
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse(strVal));
        }
        else if (type == typeof(TimeSpan))
        {
            return TimeSpan.FromSeconds(double.Parse(strVal));
        }
        else if (type.IsAssignableTo(typeof(IUser)))
        {
            return Program.Client.GetUser(ulong.Parse(strVal));
        }
        else if (type.IsAssignableTo(typeof(IChannel)))
        {
            return Program.Client.GetChannel(ulong.Parse(strVal));
        }
        else if (type.IsAssignableTo(typeof(IRole)))
        {
            if (strVal.Split('/') is { Length: 2 } split)
                return Program.Client
                    .GetGuild(split[0].To<ulong>())
                    .GetRole(split[1].To<ulong>());
        }

        try
        {
            return Convert.ChangeType(strVal, type);
        }
        catch
        {
            throw new Exception($"Conversion to type {type.FullName} is not supported");
        }
    }

    public static T ConvertFromString<T>(string strVal) => (T) ConvertFromString(strVal, typeof(T));
}