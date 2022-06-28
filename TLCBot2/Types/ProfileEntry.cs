using Microsoft.EntityFrameworkCore;

namespace TLCBot2.Types;

[PrimaryKey(nameof(UserId))]
public class ProfileEntry
{
    public ulong UserId { get; set; }
}