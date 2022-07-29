using Discord.WebSocket;
using TLCBot2.Data.RuntimeConfig;

namespace TLCBot2.Types;

public record struct RoleDescriptionPair(ulong RoleId, string Description)
{
    public SocketRole GetRole() => RuntimeConfig.FocusServer!.GetRole(RoleId);
}