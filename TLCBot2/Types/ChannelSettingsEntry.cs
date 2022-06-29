using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TLCBot2.Types;

[PrimaryKey(nameof(ChannelId))]
public class ChannelSettingsEntry
{
    public ulong ChannelId { get; set; }
    public bool DeleteMessagesWithNoAttachments { get; set; } = false;
    public bool AutoThread { get; set; } = false;
    /// <summary>
    /// A json string of a <see cref="List{T}"/> of <see cref="MessageReminder"/>
    /// </summary>
    public string MessageReminders { get; set; } = "[]";
}