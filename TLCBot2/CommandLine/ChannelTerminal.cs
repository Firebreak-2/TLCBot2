using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine;

public static partial class ChannelTerminal
{
    public static SocketTextChannel Channel;

    [PreInitialize]
    public static Task PreInitialize()
    {
        Program.Client.MessageReceived += RunCommand;
        return Task.CompletedTask;
    }

    [Initialize]
    public static async Task Initialize()
    {
        Channel = await Helper.GetChannelFromIdAsync(RuntimeConfig.TerminalChannelId);

        await Channel.SendMessageAsync($"Bot initialized at {DateTimeOffset.Now.ToDynamicTimestamp()}");
    }

    public static async Task PrintAsync(object? obj, LogSeverity? severity = null)
    {
        string colorPrefix = severity switch
        {
            LogSeverity.Error or LogSeverity.Critical => Helper.Ansi.Generate(Helper.Ansi.Foreground.Red),
            LogSeverity.Warning => Helper.Ansi.Generate(Helper.Ansi.Foreground.Yellow),
            LogSeverity.Verbose => Helper.Ansi.Generate(textModifier: Helper.Ansi.Modifier.Bold),
            _ => ""
        };
        await Channel.SendMessageAsync($"```ansi\n{colorPrefix}{obj ?? "null"}\n```");
    }
}