using Discord;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;

namespace TLCBot2.Logging;

public static partial class Log
{
    private static /*async*/ Task ToFile()
    {
        return Task.CompletedTask;
    }

    private static async Task ToChannel(Importance importance, Func<EmbedBuilder, EmbedBuilder> embed)
    {
        if (RuntimeConfig.ServerLogsChannelId is var id and not 0
            && RuntimeConfig.FocusServer!.GetTextChannel(id) is { } channel)
        {
            await channel.SendMessageAsync(embed:
                embed(new EmbedBuilder().WithColor(importance.ToColor()))
                    .Build());
        }
    }

    private enum Importance
    {
        /// <summary>
        /// Logged when something can be done maliciously, 
        /// such as deleting messages or deleting channels
        /// </summary>
        Dangerous,
        /// <summary>
        /// Logged when something needs mods to at 
        /// least be aware of, but not necessarily hurtful,
        /// such as channel creation
        /// </summary>
        Warning,
        /// <summary>
        /// Used for most logs, for normal stuff like 
        /// people editing messages, or pressing buttons,
        /// or using slash commands, etc
        /// </summary>
        Neutral,
        /// <summary>
        /// Logged when something doesnt even need to 
        /// be logged, but is logged just in case there is
        /// a need for it in the future, and to just have
        /// a general archive of everything
        /// </summary>
        Useless
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