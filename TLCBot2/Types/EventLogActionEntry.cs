using Microsoft.EntityFrameworkCore;
using TLCBot2.Logging;

namespace TLCBot2.Types;

[PrimaryKey(nameof(ID))]
public record EventLogActionEntry(string ID)
{
    public string ID { get; set; } = ID;
    /// <summary>
    /// A JSON string of a <see cref="List{T}"/> of <see cref="Log.Importance"/>
    /// </summary>
    public string? OnImportances { get; set; }
    /// <summary>
    /// A JSON string of a <see cref="List{T}"/> of <see cref="List{T}"/> of <see cref="string"/>
    /// </summary>
    public string? OnTags { get; set; }
    /// <summary>
    /// A JSON string of a <see cref="List{T}"/> of <see cref="string"/>
    /// </summary>
    public string? OnEvents { get; set; }
    /// <summary>
    /// A jump URL string leading to a message containing JSON data of a <see cref="MessageComponentAction"/>
    /// </summary>
    public string ActionLink { get; set; }
}