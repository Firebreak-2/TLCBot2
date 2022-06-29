using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TLCBot2.Types;

[PrimaryKey(nameof(UserId))]
public class ProfileEntry
{
    public ulong UserId { get; set; }
    public ushort Balance { get; set; } = 0;
    public float SocialCreditScore { get; set; } = 0;
    /// <summary>
    /// A json string of a <see cref="Dictionary{TKey,TValue}"/> of <see cref="string"/>, <see cref="string"/>
    /// </summary>
    public string SocialMedia { get; set; } = "{}";
    /// <summary>
    /// A json string of a <see cref="List{T}"/> of <see cref="ulong"/>
    /// </summary>
    public string BannedRoles { get; set; } = "[]";
}