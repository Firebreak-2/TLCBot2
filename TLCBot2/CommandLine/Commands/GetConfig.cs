using TLCBot2.Attributes;
using TLCBot2.Data;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands 
{
    [TerminalCommand(Description = "Gets the value of a runtime configuration field.")]
    public static async Task GetConfig(string field = "")
    {
        if (field != "")
        {
            // attempts to find the field;
            // if not found, throw an exception;
            // checks if the field's DisplayName is null
            // if yes, prints the field name and it's display value
            // if not, prints the field name and it's actual value
            
            string fieldValue = RuntimeConfig.Fields.TryFirst(x => string.Equals(x.Field.Name, field,
                    StringComparison.CurrentCultureIgnoreCase),
                out var f)
                ? f.Attribute.DisplayValue is null 
                    ? $"{f.Field.Name} = {f.Field.GetValue(null)}"
                    : $"{f.Field.Name} = " + f.Attribute.DisplayValue.Replace(RuntimeConfigFieldAttribute.ReplacementString, 
                        f.Field.GetValue(null)?.ToString() ?? "null") 
                : throw new Exception($"No config with the name [{field}] found");

            await ChannelTerminal.Channel.SendMessageAsync(fieldValue);
        }
        else
        {
            // does the same as above, but without checking
            // if a field exists or not, and just does the
            // process to every field the config has
            
            await ChannelTerminal.Channel.SendMessageAsync(string.Join('\n', RuntimeConfig.Fields
                .Select(x => x.Attribute.DisplayValue is null 
                    ? $"{x.Field.Name} = {x.Field.GetValue(null)}"
                    : $"{x.Field.Name} = " + x.Attribute.DisplayValue.Replace(RuntimeConfigFieldAttribute.ReplacementString,
                        x.Field.GetValue(null)?.ToString() ?? "null"))));
        }
    }
}