using Discord;
using Discord.Interactions;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [MessageCommand("Report Message")]
    public async Task ReportMessage(IMessage message)
    {
        await RespondWithModalAsync<MessageReportModal>($"message-report-modal;{message.Id},{Context.User.Id}");
    }

    public class MessageReportModal : IModal
    {
        public string Title { get; } = "Report Message";
        
        [InputLabel("Why did you report this message?")]
        [ModalTextInput("reason", TextInputStyle.Paragraph, maxLength: 2000)]
        public string Reason { get; set; }
    }

    [ModalInteraction("message-report-modal;*,*")]
    public async Task MessageReportModalResponse(string messageId, string reporterId, MessageReportModal modal)
    {
        if (RuntimeConfig.BotReportsChannel is not { } channel)
            return;
        var message = await Context.Channel.GetMessageAsync(messageId.To<ulong>());
        var reporter = await Program.Client.GetUserAsync(reporterId.To<ulong>());

        var embed = new EmbedBuilder()
            .WithTitle("Message Reported")
            .AddField("Reported By", reporter.Mention)
            .AddField("Report Reason", modal.Reason)
            .AddField("Message Author", message.Author.Mention)
            .WithColor(Color.Red)
            .WithAuthor(reporter);

        if (message.Content is {Length: > 0})
            embed.AddField("Message Content", message.Content);

        if (message.Attachments is {Count: > 0})
        {
            embed.AddField("Message Attachments", string.Join("\n", message.Attachments.Select(x => x.Url)))
                .WithImageUrl(message.Attachments.First().Url);
        }

        await channel.SendMessageAsync(embed: embed.Build());
        await RespondAsync("Report Submitted", ephemeral: true);
    }
}