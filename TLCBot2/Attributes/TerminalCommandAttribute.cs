namespace TLCBot2.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TerminalCommandAttribute : Attribute
{
    public string Description { get; set; } = "No description.";
}