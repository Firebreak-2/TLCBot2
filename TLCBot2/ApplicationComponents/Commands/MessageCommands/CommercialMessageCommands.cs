using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.DataManagement;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents.Commands.MessageCommands;

public static class CommercialMessageCommands
{
    public static async Task Initialize()
    {
        var guild = RuntimeConfig.FocusServer;
        const bool devOnly = false;

        #region Report Message Command
        await FireMessageCommand.CreateNew(new FireMessageCommand(new MessageCommandBuilder()
            .WithName("Report Message"), cmd =>
        {
            cmd.RespondWithModalAsync(new FireModal(
                new ModalBuilder("Report Message To Mods", $"modal-{Helper.RandomInt(0, 9999)}")
                    .AddTextInput(
                        "Why are you reporting this message?",
                        "tb-0",
                        TextInputStyle.Paragraph), modal =>
                {
                    var msg = (SocketUserMessage) cmd.Data.Message;
                    var embed = new EmbedBuilder()
                        .WithTitle($"Message reported by {cmd.User.Username}#{cmd.User.Discriminator}:{cmd.User.Id}")
                        .WithAuthor(msg.Author)
                        .WithColor(Color.Red)
                        .WithDescription(msg.Content)
                        .AddField("Reason For Report", modal.Data.Components.First().Value);

                    if (msg.Embeds.Any())
                        embed.WithImageUrl(msg.Embeds.First().Image!.Value.Url);
                    else if (msg.Attachments.Any())
                        embed.WithImageUrl(msg.Attachments.First().Url);

                    RuntimeConfig.FeedbackReceptionChannel.SendMessageAsync(null, false, embed.Build());
            
                    modal.RespondAsync("Message reported.", ephemeral: true);
                }).Create());
        }, devOnly), guild);
        #endregion
    }
}