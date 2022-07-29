namespace TLCBot2.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class RuntimeConfigFieldAttribute : Attribute
{
    public string? ShortName { get; set; }
    /// <summary>
    /// Where the <see cref="REPLACEMENT_STRING"/> will be replaced with
    /// the value of the field .ToString()
    /// </summary>
    public string? DisplayValue { get; set; }
    public bool Json { get; set; } = false;

    public const string REPLACEMENT_STRING = "${value}";
}