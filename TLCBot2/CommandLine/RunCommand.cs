using System.Reflection;
using Discord;
using TLCBot2.Attributes;
using TLCBot2.CommandLine.Commands;
using TLCBot2.Utilities;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace TLCBot2.CommandLine;

public static partial class ChannelTerminal
{
    public static async Task RunCommand(IMessage message)
    {
        TerminalCommands.LastCommandUser = message.Author;
        
        if (message.Channel.Id != Channel.Id)
            return;

        if (message.Author.IsBot)
            return;

        if (message.Content.StartsWith(';')) 
            return; // safe char for non-command messages
        
        if (await TryRunCommand(message) is { Length: > 0 } err)
            await PrintAsync(err, LogSeverity.Error, title: "Command Error");
    }

    /// <returns>An error message if the command fails.
    /// If the command succeeded, an empty string is returned</returns>
    private static async Task<string> TryRunCommand(IMessage message)
    {
        try
        {
            if (message.Content.Length == 0)
                return "There is no command to be executed";

            (string Name, string[]? Arguments) command = ("", null);
            {
                string[] split = message.Content.Split(' ');

                command.Name = split[0];

                if (split.Length > 1)
                    command.Arguments = split[1..];
            }

            bool Condition((MethodInfo Method, TerminalCommandAttribute Attribute) x) =>
                string.Equals(x.Method.Name, command.Name, StringComparison.CurrentCultureIgnoreCase);

            if (!TerminalCommands.All.TryFirst(Condition, out var correctCommand))
                return $"Command with name [{command.Name}] not found";

            if (command.Arguments is {Length: 1})
            {
                switch (command.Arguments[0])
                {
                    case "?":
                        await PrintAsync(
                            TerminalCommands.GenerateCommandHelpInfo(correctCommand.Method, correctCommand.Attribute),
                            title: $"Information about the [{correctCommand.Method.Name}] command");
                        return "";
                    case "\\?":
                        command.Arguments[0] = "?";
                        break;
                }
            }

            ParameterInfo[] correctParameters = correctCommand.Method.GetParameters();
            if (correctParameters.Length == 0)
            {
                correctCommand.Method.Invoke(null, null);
                return "";
            }

            object?[] providedParameters = correctParameters.Select(x => x.DefaultValue).ToArray();
            for (int i = 0; i < providedParameters.Length; i++)
            {
                if ((command.Arguments?.Length ?? 0) <= i)
                    continue;

                if (i != correctParameters.Length - 1 
                    || correctParameters.Last().ParameterType != typeof(string))
                    providedParameters[i] =
                        Helper.ConvertFromString(command.Arguments![i], correctParameters[i].ParameterType);
                else
                {
                    providedParameters[i] = string.Join(" ", command.Arguments![i..]);
                    break;
                }
            }

            await Task.Run(() => correctCommand.Method.Invoke(null, providedParameters)!);
        }
        catch (Exception e)
        {
            return e.Message;
        }

        return "";
    }
}