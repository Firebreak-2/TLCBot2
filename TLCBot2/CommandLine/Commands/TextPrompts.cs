using System.Reflection;
using System.Text;
using TLCBot2.Attributes;
using TLCBot2.Data.StringPrompts;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Change or get the value of a text prompt.")]
    public static async Task TextPrompts(string? field = null, string? newValue = null)
    {
        if (field is not null)
        {
            bool Predicate((FieldInfo Member, IEnumerable<StringPromptFieldAttribute> Attributes) x) =>
                string.Equals(x.Member.Name, field, StringComparison.CurrentCultureIgnoreCase);

            var fieldInfo = StringPrompts.Prompts.First(Predicate).Member;
            
            if (newValue is null)
            {
                await ChannelTerminal.PrintAsync(fieldInfo.GetValue(null));
            }
            else
            {
                fieldInfo.SetValue(null, newValue);
                await StringPrompts.Save();
                await ChannelTerminal.PrintAsync("Value changed");
            }
        }
        else
        {
            StringBuilder stringBuilder = new();
            for (int i = 0; i < StringPrompts.Prompts.Length; i++)
            {
                const int width = 4;
                stringBuilder.Append(StringPrompts.Prompts[i].Member.Name);
                if (i < StringPrompts.Prompts.Length - 1)
                    stringBuilder.Append(i % width == width - 1 ? "\n" : ", ");
            }

            await ChannelTerminal.PrintAsync(stringBuilder.ToString());
        }
    }
}