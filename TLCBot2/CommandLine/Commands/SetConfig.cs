using TLCBot2.Attributes;
using TLCBot2.Data;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Changes the value of a runtime configuration field.")]
    public static async Task SetConfig(string field, string newValue)
    {
        // gets the field using reflection from the given 'field'
        // parameter, and checks if its the same as the actual runtimeconfig
        // field's name or if its the same as it's short name, regardless of case
        // If no field is found, an error is thrown
        var fieldInfo = RuntimeConfig.Fields.TryFirst(x => 
                string.Equals(x.Field.Name, field,
                    StringComparison.CurrentCultureIgnoreCase) 
                || string.Equals(x.Attribute.ShortName ?? "", field,
                    StringComparison.CurrentCultureIgnoreCase), 
                out var f)
            ? f.Field
            : throw new Exception($"No config with the name [{field}] found");
        
        object val = Helper.ConvertFromString(newValue, fieldInfo.FieldType);

        string oldVal = fieldInfo.GetValue(null)?.ToString() ?? "null";
        fieldInfo.SetValue(null, val);

        await ChannelTerminal.PrintAsync($"Changed the value of {fieldInfo.Name} from {oldVal} to {val}");
    }
}