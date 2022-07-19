using Discord;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Logging.LogConfig;
using TLCBot2.Types;

namespace TLCBot2.Logging;

public static partial class Log
{
    private static async Task ToFile(LogEntry entry)
    {
        OnEventLog(entry.EventName, entry);
        
        await using var db = new TlcDbContext();
        await db.Logs.AddAsync(entry);
        await db.SaveChangesAsync();
    }

    private static async Task ToChannel(LogEntry entry, Func<EmbedBuilder, EmbedBuilder> embed)
    {
        if (!ChannelLogFilter.ShouldLog(entry))
            return;
            
        if (RuntimeConfig.ServerLogsChannel is { } channel)
        {
            await channel.SendMessageAsync(embed:
                embed(new EmbedBuilder()
                        .WithColor(((Importance) entry.Importance).ToColor()))
                        .Build());
        }
    }

    public static event EventHandler<LogEntry> OnEventLog;

    public enum Importance
    {
        /// <summary>
        /// Logged when something doesnt even need to 
        /// be logged, but is logged just in case there is
        /// a need for it in the future, and to just have
        /// a general archive of everything
        /// </summary>
        Useless = 0,
        /// <summary>
        /// Used for most logs, for normal stuff like 
        /// people editing messages, or pressing buttons,
        /// or using slash commands, etc
        /// </summary>
        Neutral = 1,
        /// <summary>
        /// Logged when something needs mods to at 
        /// least be aware of, but not necessarily hurtful,
        /// such as channel creation
        /// </summary>
        Warning = 2,
        /// <summary>
        /// Logged when something can be done maliciously, 
        /// such as deleting messages or deleting channels
        /// </summary>
        Dangerous = 3,
    }

    private static Color ToColor(this Importance importance) =>
        importance switch
        {
            Importance.Dangerous => Color.Red,
            Importance.Warning => Color.Gold,
            Importance.Neutral => Color.Blue,
            Importance.Useless => Color.DarkGrey,
            _ => throw new ArgumentOutOfRangeException(nameof(importance), importance, null)
        };
}