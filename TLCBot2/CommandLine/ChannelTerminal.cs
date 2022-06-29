using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.CommandLine.Commands;
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

    public static async Task PrintAsync(object? obj, LogSeverity? severity = null, IUser? caller = null, string? title = null, string? footnote = null)
    {
        string colorPrefix = severity switch
        {
            LogSeverity.Error or LogSeverity.Critical => Helper.Ansi.Generate(Helper.Ansi.Foreground.Red),
            LogSeverity.Warning => Helper.Ansi.Generate(Helper.Ansi.Foreground.Yellow),
            LogSeverity.Verbose => Helper.Ansi.Generate(textModifier: Helper.Ansi.Modifier.Bold),
            _ => ""
        };
        string str = $"{obj ?? "null"}";
        caller ??= TerminalCommands.LastCommandUser;

        if (caller is null || caller.ActiveClients.Any(x => x == ClientType.Mobile))
            await PrintAsync(Helper.CleanAnsiFormatting(str), "", title, footnote ?? "ANSI formatting disabled due to mobile client visibility");
        else
            await PrintAsync($"{colorPrefix}{str}", "ansi", title, footnote);
    }
    
    public static async Task PrintAsync(object? obj, string lang, string? title = null, string? footnote = null)
    {
        var builder = new EmbedBuilder();
        builder.WithDescription($"```{lang}\n{obj ?? "null"}\n```");
        if (title is { })
            builder.WithTitle(title);
        if (footnote is { })
            builder.WithFooter(footnote);
        await Channel.SendMessageAsync(null, embed: builder.Build());
    }
}