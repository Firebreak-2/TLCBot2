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
            await ChannelTerminal.Channel.SendMessageAsync(string.Join('\n', RuntimeConfig.Fields
                .Select(x => x.Attribute.DisplayValue is null 
                    ? $"{x.Field.Name} = {x.Field.GetValue(null)}"
                    : $"{x.Field.Name} = " + x.Attribute.DisplayValue.Replace(RuntimeConfigFieldAttribute.ReplacementString,
                        x.Field.GetValue(null)?.ToString() ?? "null"))));
        }
    }
}