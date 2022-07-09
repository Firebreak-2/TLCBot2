using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands
{
    public partial class InteractionCommands
    {
        [SlashCommand("poll", "Conduct a poll in chat")]
        public async Task Poll(
            string title,
            [MinValue(2)] [MaxValue(5)] int votingOptionsCount,
            bool includeOtherOption = false)
        {
            await Context.Interaction.RespondWithModalAsync<PollSettingsModal>(
                $"poll-modal;{title.Replace(' ', '`')},{includeOtherOption.AsInt()}",
                modifyModal: builder =>
                {
                    builder.Components = new();
                    for (int i = 1; i <= votingOptionsCount; i++)
                    {
                        builder.AddTextInput($"Option {i}", $"option-{i}", minLength: 1, maxLength: 100);
                    }
                });
        }

        public class PollSettingsModal : IModal
        {
            public string Title { get; } = "Poll Options";
            
            [ModalTextInput("option-1")]
            public string Option1 { get; set; }
            
            [ModalTextInput("option-2")]
            public string Option2 { get; set; }
            
            [ModalTextInput("option-3")]
            public string Option3 { get; set; }
            
            [ModalTextInput("option-4")]
            public string Option4 { get; set; }
            
            [ModalTextInput("option-5")]
            public string Option5 { get; set; }
        }

        [ModalInteraction("poll-modal;*,*")]
        public async Task PollSettingsModalResponse(string title, string hasOtherString, PollSettingsModal modal)
        {
            title = title.Replace('`', ' ');
            bool hasOther = hasOtherString.To<int>().AsBool();
            string[] optionValues = new[]
            {
                modal.Option1,
                modal.Option2,
                modal.Option3,
                modal.Option4,
                modal.Option5,
            }.Where(x => !string.IsNullOrEmpty(x))
                .ToArray();
            
            var pollData = new PollData(title,
                optionValues.Select(x =>
                    new PollData.Option(x)).ToArray());

            if (await Helper.StoreJsonData(pollData) is not { } messageId)
                return;

            var embed = GeneratePollEmbed(pollData);

            await RespondAsync(embed: embed, 
                components: new ComponentBuilder()
                    .WithSelectMenu(await GeneratePollSelectMenu(messageId, pollData))
                    .Build());
        }

        [ComponentInteraction("poll-sm;*")]
        public async Task PollSelectMenuResponse(string messageId, string[] selectedOptions)
        {
            var component = (SocketMessageComponent) Context.Interaction;
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

            await component.Message.ModifyAsync(props => props.Embed = GeneratePollEmbed(pollData));
            await Helper.StoreJsonData(pollData, id);
            
            await RespondAsync("Vote cast!", ephemeral: true);
        }

        public static async Task<SelectMenuBuilder?> GeneratePollSelectMenu(ulong messageId, PollData? data = null, string customId = "poll-sm")
        {
            data ??= (await Helper.FetchJsonData(messageId))?.FromJson<PollData>();
            if (data is null)
                return null;

            var builder = new SelectMenuBuilder()
                .WithCustomId($"{customId};{messageId}");

            foreach (var dataOption in data.Options)
            {
                builder.AddOption(dataOption.Value,
                    dataOption.Value/*.ToLower().Replace(' ', '-')*/);
            }
            
            return builder;
        }

        public static Embed GeneratePollEmbed(PollData data)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle(data.Title)
                .WithColor(Color.Blue);
            
            int totalVotes = data.Options.Sum(x => x.Votes);
            int longestVote = data.Options.MaxBy(x => $"{x.Votes}".Length)!.Votes.ToString().Length;
            
            foreach (var dataOption in data.Options)
            {
                embedBuilder.AddField(dataOption.Value, 
                    $"`{dataOption.Votes.ToString().PadRight(longestVote)} [{Helper.GenerateProgressBar(dataOption.Votes, totalVotes)}]`");
            }

            embedBuilder.WithFooter($"Total Votes: {totalVotes}");

            return embedBuilder.Build();
        }
    }
}