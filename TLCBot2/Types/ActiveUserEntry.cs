using Microsoft.EntityFrameworkCore;

namespace TLCBot2.Types;

[PrimaryKey(nameof(UserId))]
public record ActiveUserEntry(ulong UserId)
{
    public ulong UserId { get; set; } = UserId;
    public int LatestMessageCount { get; set; } = 1;
}