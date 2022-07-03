using Discord;
using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Data.RuntimeConfig;

public static partial class RuntimeConfig
{
    public static class Get
    {
        public static async Task<TChannel?> Channel<TChannel>(ulong messageId)
            where TChannel : class, IChannel =>
            await Program.Client.GetChannelAsync(messageId) as TChannel;
        
        public static async Task<TChannel?> Channel<TChannel>(string messageId)
            where TChannel : class, IChannel =>
            await Channel<TChannel>(messageId.To<ulong>());
    }
}