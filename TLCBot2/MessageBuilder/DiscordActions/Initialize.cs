using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    public static Dictionary<string, FastMethodInfo> AllActions = new();

    /// <summary>
    /// Fetch all the methods in <see cref="DiscordMethods"/> and add
    /// their parameters and action to the <see cref="AllActions"/> list.
    /// </summary>
    static DiscordMethods()
    {
        foreach (var method in typeof(DiscordMethods)
                                    .GetMethods()
                                    .Where(x => x.GetCustomAttribute<DiscordActionAttribute>() is {}))
        {
            AllActions[method.Name] = new FastMethodInfo(method.Name,
                method.GetParameters()
                    .Select(x => new FastParameterInfo(x.ParameterType, x.Name!))
                    .ToArray(),
                args => method.Invoke(null, args));
        }
    }

    public static readonly Regex ComponentIdFetcherRegex = new(@"(?<=^msgbuild-)(\w+)-(\d+\/\d+)$", RegexOptions.Compiled);

    [PreInitialize]
    public static async Task Initialize()
    {
        Program.Client.ButtonExecuted += async component =>
        {
            var match = ComponentIdFetcherRegex.Match(component.Data.CustomId);

            if (!match.Success)
                return;

            if (match.Groups[1].Value != MessageData.MessageComponentsData.ComponentData.ButtonComponentType)
            {
                await component.DeferAsync();
                return;
            }

            string actionLink = match.Groups[2].Value;

            if (actionLink == "null")
            {
                await component.DeferAsync();
                return;
            }

            if (actionLink.Split('/') is not {Length: 2} split)
            {
                await component.DeferAsync();
                return;
            }

            var channel = Program.Client.GetChannel(ulong.Parse(split[0]));
            var message = ((SocketTextChannel) channel).GetMessageAsync(ulong.Parse(split[1])).Result;

            var msg = Helper.MessageFromJumpUrl(message.GetJumpUrl()).Result;
            if (Helper.ExtractCodeFromCodeBlock(msg.Content) is var (_, code)
                && code.FromJson<MessageComponentAction>() is { } action)
            {
                action.Execute();
            }

            await component.DeferAsync();
        };
    }
}