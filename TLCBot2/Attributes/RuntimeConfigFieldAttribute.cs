namespace TLCBot2.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class RuntimeConfigFieldAttribute : Attribute
{
    public string? ShortName { get; set; }
    /// <summary>
    /// Where the <see cref="ReplacementString"/> will be replaced with
    /// the value of the field .ToString()
    /// </summary>
    public string? DisplayValue { get; set; }

    public const string ReplacementString = "${value}";
}