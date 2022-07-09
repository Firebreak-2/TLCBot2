using Microsoft.EntityFrameworkCore;

namespace TLCBot2.Types;

[PrimaryKey(nameof(InviteId))]
public record ServerInviteEntry(string InviteId)
{
    public string InviteId { get; set; } = InviteId;
    public int Uses { get; set; }
}