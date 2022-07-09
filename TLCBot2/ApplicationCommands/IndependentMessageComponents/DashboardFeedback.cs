using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Data.StringPrompts;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [ComponentInteraction("feedback-button;*")]
    public async Task DashboardFeedbackSelectMenuResponse(string selectedOption)
    {
        switch (selectedOption)
        {
            case "b": // Bug Report
            {
                await RespondWithModalAsync<BugReportsModal>("bug-report-modal");
                return;
            }
            case "s": // Server Suggestion
            {
                await RespondWithModalAsync<ServerSuggestionModal>("server-suggestion-modal");
                return;
            }
            case "q": // QOTD Suggestion
            {
                await RespondWithModalAsync<QotdSuggestionModal>("qotd-suggestion-modal");
                return;
            }
        }

        await DeferAsync();
    }

    public class QotdSuggestionModal : IModal
    {
        public string Title { get; } = "Suggest a QOTD";

        [InputLabel("What is your art-related QOTD?")]
        [ModalTextInput("qotd-suggestion")]
        public string Message { get; set; }
    }

    [ModalInteraction("qotd-suggestion-modal")]
    public async Task QotdSuggestionModalResponse(QotdSuggestionModal modal)
    {
        if (RuntimeConfig.UserFeedbackChannel is not { } channel)
            throw new Exception("User feedback channel not provided");
        
        var embed = new EmbedBuilder()
            .WithTitle("QOTD Suggestion")
            .WithColor(Color.Blue)
            .WithAuthor(Context.User)
            .AddField("Sent From", Context.User.Mention)
            .AddField("Their QOTD", modal.Message)
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Post", "post-qotd-button;send")
            .WithButton("Edit and Post", "post-qotd-button;edit")
            .Build();

        await channel.SendMessageAsync(embed: embed, components: components);

        await RespondAsync("QOTD Suggestion submitted!", ephemeral: true);
    }
    
    public class BugReportsModal : IModal
    {
        public string Title { get; } = "Report A Bug";

        [InputLabel("Explain the issue you encountered")]
        [ModalTextInput("bug-message", TextInputStyle.Paragraph)]
        public string BugMessage { get; set; }
    }

    [ModalInteraction("bug-report-modal")]
    public async Task BugReportModalResponse(BugReportsModal modal)
    {
        if (RuntimeConfig.UserFeedbackChannel is not { } channel)
            return;
        
        await channel.SendMessageAsync(embed: new EmbedBuilder()
            .WithTitle("Bug Reported")
            .WithColor(Color.Red)
            .WithAuthor(Context.User)
            .AddField("Sent From", Context.User.Mention)
            .AddField("What They Reported", modal.BugMessage)
            .Build());
        
        await RespondAsync("Bug Reported", ephemeral: true);
    }

    public class ServerSuggestionModal : IModal
    {
        public string Title { get; } = "Server Suggestion";

        [InputLabel("Suggestion Title")]
        [ModalTextInput("suggestion-title")]
        public string SuggestionTitle { get; set; }
        
        [RequiredInput(false)]
        [InputLabel("Suggestion Description (optional)")]
        [ModalTextInput("suggestion-description", TextInputStyle.Paragraph)]
        public string? SuggestionDescription { get; set; }
        
        [RequiredInput(false)]
        [InputLabel("Attached Image Link (optional)")]
        [ModalTextInput("suggestion-image")]
        public string? SuggestionAttachment { get; set; }
    }

    [ModalInteraction("server-suggestion-modal")]
    public async Task ServerSuggestionModalResponse(ServerSuggestionModal modal)
    {
        if (RuntimeConfig.ServerSuggestionsChannel is not { } channel)
            return;

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithAuthor(Context.User)
            .WithTitle(modal.SuggestionTitle)
            .WithFooter(StringPrompts.ServerSuggestionsHiddenVoteReason);

        if (modal.SuggestionDescription is { })
            embed.WithDescription($"**{modal.SuggestionDescription}**");
        if (modal.SuggestionAttachment is { })
            embed.WithImageUrl(modal.SuggestionAttachment);
        
        var pollData = new PollData(modal.SuggestionTitle, new[]
        {
            new PollData.Option("Agree"),
            new PollData.Option("Disagree")
        });

        if (await Helper.StoreJsonData(pollData) is not { } messageId)
            return;

        var components = new ComponentBuilder()
            .WithSelectMenu((await GeneratePollSelectMenu(messageId, pollData, "server-suggestions-vote-sm"))
                .WithPlaceholder("Vote"))
            .WithButton("Show Votes", $"server-suggestion-show-votes-button;{messageId}");

        await channel.SendMessageAsync(
            embed: embed.Build(),
            components: components.Build());
        
        await RespondAsync("Suggestion Sent!", ephemeral: true);
    }
}