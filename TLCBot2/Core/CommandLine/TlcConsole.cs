using System.Text.RegularExpressions;
using Discord.WebSocket;
using TLCBot2.Utilities;

namespace TLCBot2.Core.CommandLine;

public static class TlcConsole
{
    public static ISocketMessageChannel Channel = RuntimeConfig.TLCBetaCommandLine;
    public static List<TlcCommand> ListCommand = new();
    public static Task OnMessageRecieved(SocketMessage message)
    {
        if (message.Channel.Id != Channel.Id
            || message.Author.IsBot)
            return Task.CompletedTask;

        if (!RunCommand(message.Content))
        {
            Print($"command not found: {message.Content.Split(' ')[0].Replace("`", "")}");
        }
        
        return Task.CompletedTask;
    }

    public static bool RunCommand(string command)
    {
        try
        {
            command = Regex.Replace(command, @"\/\*.*?\*\/|\/\/[^\n]*", "", RegexOptions.Singleline);
            if (command.Length == 0) return true;
            
            var split = command.Split(' ');
            var cmdName = split[0];
            split = split[1..];

            bool Condition(TlcCommand x) => string.Equals(x.Name, cmdName, StringComparison.CurrentCultureIgnoreCase);
            if (ListCommand.All(x => !Condition(x)))
                return false;
            var cmd = ListCommand.First(Condition);

            if (cmd.Args != -1 && cmd.Args > 0)
            {
                var idx = cmd.Args - 1;
                var finale = string.Join(" ", split[idx..]);
                split = split[..idx].Append(finale).ToArray();
            }

            cmd.Invoke(split);
        }
        catch (Exception e)
        {
            Print(e.Message);
        }

        return true;
    }

    public static void Print(object? obj = null, string language = "")
    {
        var str = obj?.ToString() ?? "null";
        Channel.SendMessageAsync($"```{language}\n{str}\n```");
    }
}