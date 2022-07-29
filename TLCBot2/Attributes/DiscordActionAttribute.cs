using TLCBot2.MessageBuilder.DiscordActions;

namespace TLCBot2.Attributes;

/// <summary>
/// Marks a method as a Discord Action when declared
/// in the <see cref="DiscordMethods"/> partial class
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DiscordActionAttribute : Attribute
{
    public string[] ExternalParameterNames { get; set; } = Array.Empty<string>();

    public bool NoDefer { get; set; } = false;
}