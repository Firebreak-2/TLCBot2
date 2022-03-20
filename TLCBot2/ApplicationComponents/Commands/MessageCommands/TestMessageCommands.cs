using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.DataManagement;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents.Commands.MessageCommands;

public static class TestMessageCommands
{
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;

        await FireMessageCommand.CreateNew(new FireMessageCommand(new MessageCommandBuilder()
            .WithName("Edit Message"), cmd =>
        {
            var self = Program.BetaClient.CurrentUser;
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
        }, true), guild);

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
        }), guild);
    }
}