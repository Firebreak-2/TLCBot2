using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [ComponentInteraction("post-qotd-button;*")]
    public async Task QotdPostButtonResponse(string mode)
    {
        var message = (SocketUserMessage) ((IComponentInteraction) Context.Interaction).Message;
        
        if (mode == "send")
        {
            await RespondAsync(await DoStuff(message), ephemeral: true);
        }
        else if (mode == "edit")
        {
            await Context.Interaction.RespondWithModalAsync<EditQotdAndPostModal>(
                $"edit-qotd-modal;{message.Channel.Id}/{message.Id}",
                modifyModal: modal =>
                {
                    string question = message.Embeds.First().Fields.First(x => x.Name == "Their QOTD").Value;
                    modal.Components = new ModalComponentBuilder()
                        .WithTextInput("The new value for the question", "new-qotd", value: question);
                });
        }
    }

    public class EditQotdAndPostModal : IModal
    {
        public string Title { get; set; } = "Edit and Post";
        
        [InputLabel("The new value for the question")]
        [ModalTextInput("new-qotd")]
        public string NewQotd { get; set; }
    }

    [ModalInteraction("edit-qotd-modal;*")]
    public async Task EditAndPostModalResponse(string messageLink, EditQotdAndPostModal modal)
    {
        IUserMessage message;
        {
            string[] split = messageLink.Split('/');
            message = (IUserMessage) await ((ISocketMessageChannel) await Program.Client.GetChannelAsync(split[0].To<ulong>()))
                .GetMessageAsync(split[1].To<ulong>());
        }

        await RespondAsync(await DoStuff(message, modal.NewQotd), ephemeral: true);
    }

    private static async Task<string> DoStuff(IUserMessage message, string? question = null)
    {
        if (RuntimeConfig.QotdChannel is not { } channel)
            throw new Exception("QOTD channel value not set");

        if (RuntimeConfig.QotdRole is not { } role)
            throw new Exception("QOTD role value not set");
        
        question ??= message.Embeds.First().Fields.First(x => x.Name == "Their QOTD").Value;
        IUser poster = await Program.Client.GetUserAsync(message.Embeds.First().Fields.First(x => x.Name == "Sent From").Value[3..^1].To<ulong>());
            
        var msg = await channel.SendMessageAsync(
            $"{role.Mention} {question}\n\n" +
            $"Thanks to {poster.Mention} for suggesting this question.");
            
        await Helper.DisableMessageComponentsAsync(message);

        return $"Posted question on {RuntimeConfig.QotdChannel.Mention}\n\ngo to: {msg.GetJumpUrl()}";
    }
}