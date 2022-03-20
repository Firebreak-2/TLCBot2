using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents.Commands.MessageCommands;

public class AdminMessageCommands
{
    
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;
        const bool devOnly = false;

        #region Edit Message Command
        await FireMessageCommand.CreateNew(new FireMessageCommand(new MessageCommandBuilder()
            .WithName("Edit Message"), cmd =>
        {
            var self = Program.Client.CurrentUser;
            if (cmd.Data.Message.Author.Id != self.Id)
            {
                cmd.RespondAsync($"Cannot edit message not sent by `{self.Mention}`", ephemeral: true);
                return;
            }

            cmd.RespondWithModalAsync(new FireModal(new ModalBuilder("Edit Message", $"modal-{Helper.RandomInt(0, 9999)}")
                .AddTextInput("New Message", "tb-0", TextInputStyle.Paragraph), modal =>
            {
                string newContent = modal.Data.Components.First().Value;
                ((SocketUserMessage) cmd.Data.Message).ModifyAsync(props => props.Content = newContent);
                modal.RespondAsync();
            }).Create());
        }, devOnly), guild);
        #endregion
    }
}