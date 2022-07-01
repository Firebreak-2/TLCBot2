using System.Reflection;
using System.Text;
using TLCBot2.Attributes;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Lists each terminal command with it's parameters and a description on the command.")]
    public static async Task Help()
    {
        // prints every command in the terminal with a description
        // generated with the GenerateCommandHelpInfo method
        await ChannelTerminal.PrintAsync(
            string.Join("\n\n", All.Select(x => GenerateCommandHelpInfo(x.Method, x.Attribute))),
            title: "Terminal Command Help");
    }

    public static string GenerateCommandHelpInfo(MethodInfo commandMethodInfo, TerminalCommandAttribute attribute)
    {
        string white =  Helper.Ansi.Foreground.White.Get();
        string green =  Helper.Ansi.Foreground.Green.Get();
        string yellow = Helper.Ansi.Foreground.Yellow.Get();
        string cyan =   Helper.Ansi.Foreground.Cyan.Get();
        
        int indentLevel = 0;
        StringBuilder stringBuilder = new();

        // generates a string that looks like: MethodName(ParameterType ParameterName)
        // with ansi coloring, such that it looks like it has syntax highlighting
        AddString($"{green}{commandMethodInfo.Name}{white}({string.Join($"{white}, ", commandMethodInfo.GetParameters().Select(x => $"{green}{x.ParameterType.Name} {yellow}{x.Name}"))}{white})\n");
        indentLevel++;
        
        // shows the description of the command, and splits it
        // into multiple lines if its too long
        stringBuilder.Append(cyan);
        string[] splitDescription = Helper.ApplyLineBreaks(attribute.Description).Split('\n');
        foreach (string descriptionSegment in splitDescription)
        {
            AddString($"// {descriptionSegment}\n");
        }

        return stringBuilder.ToString();

        // appends a string to the string builder with
        // consideration to the indent level
        void AddString(string str)
        {
            stringBuilder.Append($"{new string(' ', 2 * indentLevel)}{str}");
        }
    }
}