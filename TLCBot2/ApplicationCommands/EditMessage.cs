using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [MessageCommand("Edit Message")]
    public async Task EditMessage(SocketUserMessage message)
    {
        if (message.Author.Id != Program.Client.CurrentUser.Id)
            return; // not tlc bot

        await Context.Interaction.RespondWithModalAsync<EditMessageModal>(
            $"edit-message-modal;{message.Id}",
            modifyModal: builder =>
            {
                // changes the text box to contain the data i need it to
                // have but that attributes dont support (dynamic values)
                builder.Components.ActionRows[0].Components[0] = 
                    new TextInputBuilder("New Message Content", "0", TextInputStyle.Paragraph, value: message.Content)
                        .Build();
            });
    }

    public class EditMessageModal : IModal
    {
        public string Title { get; } = "Edit Message";
        
        [ModalTextInput("0")]
        public string NewContent { get; set; }
    }
    
    [ModalInteraction("edit-message-modal;*")]
    public async Task EditMessageModalResponse(string id, EditMessageModal modal)
    {
        ulong messageId = ulong.Parse(id);

        await Context.Channel.ModifyMessageAsync(messageId, 
            props => props.Content = modal.NewContent);

        await RespondAsync();
    }
}