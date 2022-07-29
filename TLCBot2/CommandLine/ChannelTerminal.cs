using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.CommandLine.Commands;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
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

    [Initialize(Priority = 999)]
    public static async Task Initialize()
    {
        Channel = await Helper.GetChannelFromIdAsync(RuntimeConfig.TerminalChannelId);
    }

    [Initialize(Priority = 0)]
    public static async Task PostInitialize()
    {
        // this is quite slow because of uncached reflection get value methods
        // but it should be ok since it only runs once when the bot starts up
        
        string missingValues = RuntimeConfig.Fields.Any(x => x.Field.GetValue(null) is null) 
            ? $"\n" +
              $"⚠️ Missing Runtime Config Fields ⚠️\n" +
              $"{string.Join('\n', RuntimeConfig.Fields.Where(x => x.Field.GetValue(null) is null).Select(x => $"[{x.Field.Name}]"))}"
            : "";

        await Channel.SendMessageAsync($"Bot initialized at {DateTimeOffset.Now.ToDynamicTimestamp()}{missingValues}");
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

        if (caller is null || caller.OnMobile())
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