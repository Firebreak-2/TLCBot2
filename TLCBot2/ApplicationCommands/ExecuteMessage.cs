using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("execute-message", "Generates a message from the given JSON data")]
    public async Task ExecuteMessageSlashCommand(string fromMessageUrl)
    {
        if (await Helper.MessageFromJumpUrl(fromMessageUrl) is { } message)
        {
            var match = Helper.ExtractCodeFromCodeBlock(message.Content);
            if (match is null or { Language: not null and not "json" })
            {
                await RespondAsync("Not a valid JSON format", ephemeral: true);
                return;
            }

            await ExecuteMessageFromString(match.Value.Code);
        }
        else
        {
            await RespondAsync("Message does not exist", ephemeral: true);
        }
    }

    [MessageCommand("Execute Message")]
    public async Task ExecuteMessageMessageCommand(IMessage message)
    {
        await ExecuteMessageSlashCommand(message.GetJumpUrl());
    }
    
    private async Task ExecuteMessageFromString(string json)
    {
        try
        {
            if (JsonConvert.DeserializeObject<MessageComponentAction>(json) is { } action)
            {
                await RespondAsync(action.Execute<object>(null).ToJson().ToCodeBlock(), ephemeral: true);
            }
        }
        catch (Exception e)
        {
            await RespondAsync(e.Message, ephemeral: true);
        }
    }
}