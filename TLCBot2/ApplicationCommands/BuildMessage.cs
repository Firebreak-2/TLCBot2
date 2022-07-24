using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("build-message", "Generates a message from the given JSON data")]
    public async Task BuildMessageSlashCommand(IMessageChannel channel, string fromMessageUrl = "")
    {
        if (fromMessageUrl == "")
        {
            await RespondWithModalAsync<MessageBuilderModal>($"message-builder-modal;{channel.Id}");
        }
        else if (await Helper.MessageFromJumpUrl(fromMessageUrl) is { } message)
        {
            var match = Helper.ExtractCodeFromCodeBlock(message.Content);
            if (match is null or { Language: not null and not "json" })
            {
                await RespondAsync("Not a valid JSON format", ephemeral: true);
                return;
            }
            await SendMessageFromString(channel.Id, match.Value.Code);
            await RespondAsync("Message sent", ephemeral: true);
        }
    }

    [MessageCommand("Build Message")]
    public async Task BuildMessageMessageCommand(IMessage message)
    {
        await BuildMessageSlashCommand(message.Channel, message.GetJumpUrl());
    }
    
    public class MessageBuilderModal : IModal
    {
        public string Title { get; } = "Message Builder";

        [InputLabel("Message JSON")]
        [ModalTextInput("parameter-id", TextInputStyle.Paragraph)]
        public string MessageJson { get; set; }
    }
    
    [ModalInteraction("message-builder-modal;*")]
    public async Task ModalResponse(string channelId, MessageBuilderModal modal)
    {
        await SendMessageFromString(channelId.To<ulong>(), modal.MessageJson);
        await RespondAsync("Message sent", ephemeral: true);
    }
    
    private async Task SendMessageFromString(ulong channelId, string json)
    {
        // deserialize the json data and try sending it.
        try
        {
            if (JsonConvert.DeserializeObject<MessageData>(json) is { } messageData)
            {
                await messageData.SendAsync(channelId);
            }
        }
        catch (Exception e)
        {
            await RespondAsync(e.ToString(), ephemeral: true);
        }
    }
}