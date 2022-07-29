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
    public static Dictionary<string, (FastMethodInfo method, DiscordActionAttribute attribute)> AllActions = new();

    /// <summary>
    /// Fetch all the methods in <see cref="DiscordMethods"/> and add
    /// their parameters and action to the <see cref="AllActions"/> list.
    /// </summary>
    static DiscordMethods()
    {
        foreach (var (method, attributes) in Helper.GetAllMembersWithAttribute<MethodInfo, DiscordActionAttribute>())
        {
            var attribute = attributes.First();

            var parameters = method.GetParameters();
            
            AllActions[method.Name] = (new FastMethodInfo(method.Name,
                parameters
                    .Select(x => new FastParameterInfo(x.ParameterType, x.Name!))
                    .ToArray(),
                args => method.Invoke(null, args?
                    .Select((x, i) => parameters[i].ParameterType == typeof(string) 
                        ? x?.ToString() 
                        : x)
                    .ToArray())), attribute);
        }
    }

    public static readonly Regex ComponentIdFetcherRegex = new(@"(?<=^msgbuild-)(\w+)-(\d+\/\d+)$", RegexOptions.Compiled);

    [PreInitialize]
    public static async Task Initialize()
    {
        Program.Client.ButtonExecuted += onComponentExecuted;
        Program.Client.SelectMenuExecuted += onComponentExecuted;

        async Task onComponentExecuted(SocketMessageComponent component)
        {
            var match = ComponentIdFetcherRegex.Match(component.Data.CustomId);

            if (!match.Success)
                return;

            if ((component.Data.Type == ComponentType.Button 
                    && match.Groups[1].Value != 
                        MessageData.MessageComponentsData.ComponentData.ButtonComponentType)
                || (component.Data.Type == ComponentType.SelectMenu
                    && match.Groups[1].Value !=
                        MessageData.MessageComponentsData.ComponentData.SelectMenuComponentType))
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

            string[] args = Array.Empty<string>();
            
            var msg = await Helper.MessageFromJumpUrl(message.GetJumpUrl());
            if (Helper.ExtractCodeFromCodeBlock(msg.Content) is var (_, code)
                && code.FromJson<MessageComponentAction>() is { } action)
            {
                args = (component.Data.Values is {Count: > 0}
                    ? action.Execute<object>(component, ("$%selectedOptions%", component.Data.Values.ToArray()))
                    : action.Execute<object>(component)).Arguments;
            }

            if (!args.Contains("nodefer"))
                await component.DeferAsync();
        }
    }
}