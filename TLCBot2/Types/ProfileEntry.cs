﻿using Microsoft.EntityFrameworkCore;

namespace TLCBot2.Types;

[PrimaryKey(nameof(UserId))]
public record ProfileEntry
{
    public ulong UserId { get; set; }
    public int Balance { get; set; } = 0;
    public float SocialCreditScore { get; set; } = 0;
    /// <summary>
    /// A json string of a <see cref="Dictionary{TKey,TValue}"/> of <see cref="string"/>, <see cref="string"/>
    /// </summary>
    public string SocialMedia { get; set; } = "{}";
    /// <summary>
    /// A json string of a <see cref="List{T}"/> of <see cref="ulong"/>
    /// </summary>
    public string BannedRoles { get; set; } = "[]";
    public string? ModNote { get; set; }
}