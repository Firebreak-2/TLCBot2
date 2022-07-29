using System.Globalization;
using Discord;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;

namespace TLCBot2.Utilities;

public static partial class Helper
{
    public static object? ConvertFromString(string? strVal, Type type)
    {
        // i would have used a switch statement
        // for this but unfortunately doing
        // typeof(T) does not work in switch cases
        // because it says it's not a compile-time
        // constant. so here we are stuck with this
        // abomination.

        if (strVal is null)
            return null;
        else if (type.IsAssignableTo(typeof(Enum)))
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
            var match = MentionDeFormatRegex.Match(strVal);
            if (!match.Success)
                return Program.Client.GetUser(ulong.Parse(strVal));

            if (match.Groups[1].Value[0] != '@')
                throw new Exception("Not a valid user");
            
            return Program.Client.GetUser(ulong.Parse(match.Groups[2].Value));
        }
        else if (type.IsAssignableTo(typeof(IChannel)))
        {
            var match = MentionDeFormatRegex.Match(strVal);
            if (!match.Success)
                return Program.Client.GetChannel(ulong.Parse(strVal));

            if (match.Groups[1].Value != "#")
                throw new Exception("Not a valid channel");
            
            return Program.Client.GetChannel(ulong.Parse(match.Groups[2].Value));
        }
        else if (type.IsAssignableTo(typeof(IRole)))
        {
            var match = MentionDeFormatRegex.Match(strVal);
            if (!match.Success && strVal.Split('/') is { Length: 2 } split)
                return Program.Client
                    .GetGuild(split[0].To<ulong>())
                    .GetRole(split[1].To<ulong>());
            
            if (match.Groups[1].Value != "@&")
                throw new Exception("Not a valid role");
            
            return RuntimeConfig.FocusServer!.GetRole(match.Groups[2].Value.To<ulong>());
        }
        else if (type.IsAssignableTo(typeof(IGuild)))
        {
            return Program.Client.GetGuild(ulong.Parse(strVal));
        }
        else if (type.IsAssignableTo(typeof(IMessage)))
        {
            return Task.Run(async () => await MessageFromJumpUrl(strVal)).Result;
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
    
    public static string? ConvertToString(object? obj) =>
        obj switch
        {
            Enum @enum => @enum.ToString(),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToUnixTimeSeconds().ToString(),
            TimeSpan timeSpan => timeSpan.TotalSeconds.ToString(CultureInfo.CurrentCulture),
            IUser user => user.Id.ToString(),
            IChannel channel => channel.Id.ToString(),
            IRole role => $"{role.Guild.Id}/{role.Id}",
            IGuild guild => guild.Id.ToString(),
            _ => obj?.ToJson()
        };

    public static T? ConvertFromString<T>(string strVal) => (T?) ConvertFromString(strVal, typeof(T));
}