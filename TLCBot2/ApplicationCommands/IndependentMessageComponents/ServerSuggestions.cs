using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [ComponentInteraction("server-suggestions-vote-sm;*")]
    public async Task ServerSuggestionVoteSelectMenu(string messageId, string[] selectedOptions)
    {
        ulong id = messageId.To<ulong>();
        if (await Helper.FetchJsonData<PollData>(id) is not { } pollData)
            return;

        SocketUser voter = Context.User;
        string votedFor = selectedOptions[0];
            
        if (pollData.VoterIds.Contains(voter.Id) && !Program.DeveloperMode)
        {
            await RespondAsync("Cannot vote more than once", ephemeral: true);
            return;
        }
            
        pollData.Options.First(x => x.Value == votedFor).Votes++;
            
        if (!Program.DeveloperMode)
            pollData.VoterIds.Add(voter.Id);

        await Helper.StoreJsonData(pollData, id);
        await RespondAsync("Vote cast!", ephemeral: true);
    }
    
    [ComponentInteraction("server-suggestion-show-votes-button;*")]
    public async Task ServerSuggestionShowVotesButton(string messageId)
    {
        ulong id = messageId.To<ulong>();
        if (await Helper.FetchJsonData<PollData>(id) is not { } pollData)
            return;

        var message = ((IComponentInteraction) Context.Interaction).Message;

        MessageComponent? components = null;
        if (((SocketGuildUser) Context.User).Roles.Contains(RuntimeConfig.CanAcceptSuggestionsRole))
        {
            components = new ComponentBuilder()
                .WithButton("Accept", $"server-suggestions-mod-action;a,{message.Id}", ButtonStyle.Success)
                .WithButton("Reject", $"server-suggestions-mod-action;r,{message.Id}", ButtonStyle.Danger)
                .Build();
        }
        
        if (pollData.VoterIds.Contains(Context.User.Id) && !Program.DeveloperMode)
            await RespondAsync(embed: GeneratePollEmbed(pollData), ephemeral: true, components: components);
        else
            await RespondAsync("Cant view votes unless you've already voted", ephemeral: true);
    }
    
    [ComponentInteraction("server-suggestions-mod-action;*,*")]
    public async Task ServerSuggestionModActionButton(string action, string messageId)
    {
        if (RuntimeConfig.ServerSuggestionsChannel is not { } channel)
            throw new Exception("Server Suggestions channel not set");

        var message = (IUserMessage) await channel.GetMessageAsync(messageId.To<ulong>());

        if (DateTimeOffset.Now < message.Timestamp + TimeSpan.FromDays(1) && !Program.DeveloperMode)
        {
            await RespondAsync("Suggestion has to be at least 1 day old before an action can be taken", ephemeral: true);
            return;
        }

        string id = $"server-suggestion-mod-action-modal;{message.Id},{action}";
        
        await RespondWithModalAsync<ServerSuggestionModActionModal>(id);
    }

    public class ServerSuggestionModActionModal : IModal
    {
        public string Title { get; } = "an interesting tit";
        
        [InputLabel("What is the reason for this?")]
        [ModalTextInput("reason", TextInputStyle.Paragraph)]
        public string Reason { get; set; }
    }

    private static readonly Regex _getServerSuggestionTitleRegex = new(@"[✅❎\s]+\b(.+)", RegexOptions.Compiled);

    [ModalInteraction("server-suggestion-mod-action-modal;*,*")]
    public async Task ServerSuggestionModActionModalResponse(string messageId, string action, ServerSuggestionModActionModal modal)
    {
        var message = (IUserMessage) await RuntimeConfig.ServerSuggestionsChannel!
            .GetMessageAsync(messageId.To<ulong>());

        var embed = message.Embeds.First().ToEmbedBuilder();
        string title = _getServerSuggestionTitleRegex.Match(embed.Title).Groups[1].Value;

        switch (action)
        {
            case "a": // Accept
            {
                embed.WithColor(Color.Green);
                embed.Title = $"✅ {title}";
                embed.Fields.RemoveAll(x => x.Name is "Accepted" or "Rejected");
                embed.AddField("Accepted", modal.Reason);
                break;
            }
            case "r": // Reject
            {
                embed.WithColor(Color.Red);
                embed.Title = $"❎ {title}";
                embed.Fields.RemoveAll(x => x.Name is "Accepted" or "Rejected");
                embed.AddField("Rejected", modal.Reason);
                break;
            }
        }

        await message.ModifyAsync(props => props.Embed = embed.Build());
        await DeferAsync();
    }
}