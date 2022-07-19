using Microsoft.EntityFrameworkCore;

namespace TLCBot2.Types;

[PrimaryKey(nameof(ChannelId))]
public record ChannelSettingsEntry
{
    public ulong ChannelId { get; set; }
    public bool DeleteMessagesWithNoAttachments { get; set; } = false;
    public bool AutoThread { get; set; } = false;
    /// <summary>
    /// A json string of a <see cref="List{T}"/> of <see cref="String"/> where each
    /// item is an emoji to react with
    /// </summary>
    public string AutoReact { get; set; } = "[]";

    /// <summary>
    /// A json string of a <see cref="List{T}"/> of <see cref="MessageReminder"/>
    /// </summary>
    public string? MessagesMustMatchPattern { get; set; } = null;
    public string MessageReminders { get; set; } = "[]";
}