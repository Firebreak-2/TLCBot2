using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.DataManagement.Temporary;
using TLCBot2.Utilities;
using Color = Discord.Color;

namespace TLCBot2.ApplicationComponents.Commands.SlashCommands;

public static class TestSlashCommands
{
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;

        #region Dynamic Timestamp Generator Command
        await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
            .WithName("generate-timestamp")
            .WithDescription("Generates a dynamic timestamp to use")
            .AddOption(
                "hour-offset",
                ApplicationCommandOptionType.Integer, 
                "The UTC time offset from your timezone (yourTimezone = UTC+{offset})",
                minValue: -24, maxValue: 24)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("style")
                .WithDescription("The style of the timestamp to be displayed in")
                .WithType(ApplicationCommandOptionType.String)
                .AddChoice("Short Time", "t")
                .AddChoice("Long Time", "T")
                .AddChoice("Short Date", "d")
                .AddChoice("Long Date", "D")
                .AddChoice("Long Date with Short Time", "f")
                .AddChoice("Weekday with Long Date with Short Time", "F")
                .AddChoice("Relative Time", "R")
            )
            .AddOption("year", ApplicationCommandOptionType.Integer, "The specified [year] in the UTC timezone")
            .AddOption("month", ApplicationCommandOptionType.Integer, "The specified [month] in the UTC timezone")
            .AddOption("day", ApplicationCommandOptionType.Integer, "The specified [day] in the UTC timezone")
            .AddOption("hour", ApplicationCommandOptionType.Integer, "The specified [hour] in the UTC timezone")
            .AddOption("minute", ApplicationCommandOptionType.Integer, "The specified [minute] in the UTC timezone")
            .AddOption("second", ApplicationCommandOptionType.Integer, "The specified [second] in the UTC timezone")
            , cmd =>
            {
                var offset = new TimeSpan(Convert.ToInt32(cmd.Data.Options.First(x => x.Name == "hour-offset").Value), 0, 0);
                var now = DateTimeOffset.UtcNow + offset;
        
                bool Condition(SocketSlashCommandDataOption x, string x2) => x.Name == x2;
                int GetOpt(string name, int defaultValue)
                {
                    return cmd.Data.Options.Any(x => Condition(x, name))
                        ? Convert.ToInt32(cmd.Data.Options.First(x => Condition(x, name)).Value)
                        : defaultValue;
                }
        
                string style = cmd.Data.Options.Any(x => Condition(x, "style"))
                    ? (string) cmd.Data.Options.First(x => Condition(x, "style")).Value
                    : "Long Date with Short Time";
                
                int year = GetOpt("year", now.Year);
                int month = GetOpt("month", now.Month);
                int day = GetOpt("day", now.Day);
                int hour = GetOpt("hour", now.Hour);
                int minute = GetOpt("minute", now.Minute);
                int second = GetOpt("second", now.Second);
                
                string timestamp = $"<t:{new DateTimeOffset(year, month, day, hour, minute, second, 0, offset).ToUnixTimeSeconds()}:{style}>";
                cmd.RespondAsync($"`{timestamp}` {timestamp}");
            }), guild);
        #endregion

        #region Spawn Button Command
        await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
            .WithName("spawn-button")
            .WithDescription("spawns a button"), cmd =>
        {
            
        }, true), guild);
        #endregion

        #region Poll Command
        await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
            .WithName("poll")
            .WithDescription("Conducts a poll")
            .AddOption("title", ApplicationCommandOptionType.String, "The title(question) of the poll", true)
            .AddOption("options", ApplicationCommandOptionType.Integer, "The amount of options (2-5)", true, minValue:2, maxValue:5)
            .AddOption("anonymous", ApplicationCommandOptionType.Boolean, "Whether you want to make it public that you made this poll"),
            cmd =>
            {
                string title = (string) cmd.Data.Options.First(x => x.Name == "title").Value;
                long optionCount = (long) cmd.Data.Options.First(x => x.Name == "options").Value;
                bool anonymous = cmd.Data.Options.Count <= 2 || (bool)cmd.Data.Options.First(x => x.Name == "anonymous").Value;
                
                var modalBuilder = new ModalBuilder()
                    .WithTitle("Poll Options")
                    .WithCustomId($"modal-{Helper.RandomInt(0, 1000)}");
                
                for (int i = 0; i < optionCount; i++)
                {
                    modalBuilder.AddTextInput($"Option {i+1}", $"{i}");
                }
                
                var builtModal = FireModal.CreateNew(new FireModal(modalBuilder, modal =>
                {
                    var options = new PollHandler.PollOption[optionCount];
                    foreach (var item in modal.Data.Components)
                    {
                        int index = int.Parse(item.CustomId);
                        options[index] = new PollHandler.PollOption(item.Value);
                    }
                    
                    var poll = new PollHandler.Poll(title, $"poll-{Helper.RandomInt(0, 1000)}", options);
                    EmbedBuilder GenerateEmbed()
                    {
                        var embed = new EmbedBuilder().WithTitle(title).WithColor(Color.Blue);
                        int allVotes = poll.Options.Sum(opt => opt.Votes);
                        foreach (var option in poll.Options)
                        {
                            const int charCount = 20;
                            const char w = '#';
                            const char b = '-';
                            float votePercent = (allVotes > 0 ? (float)option.Votes / allVotes : option.Votes) * charCount;

                            string whiteBar = $"{(votePercent > 0 ? new string(w, (int)votePercent) : "")}";
                            string blackBar = $"{new string(b, (int)(charCount - votePercent))}";
                            string fullBar = $"{whiteBar}{blackBar}";
                            fullBar = fullBar.Length == charCount - 1 ? fullBar + b : fullBar;
                            string spacing = new(' ',
                                allVotes.ToString().Length - option.Votes.ToString().Length);
                            embed.AddField(option.Title, $"`{option.Votes + spacing} {fullBar}`");
                        }

                        embed.WithFooter($"Total votes: {allVotes}");

                        if (!anonymous)
                            embed.WithAuthor(cmd.User);
    
                        return embed;
                    }
                
                    var cb = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                        .WithSelectMenu(poll.CustomID, poll.Options.Select(option => new SelectMenuOptionBuilder()
                            .WithLabel(option.Title)
                            .WithValue(option.Title)).ToList(), maxValues: 1), null, selectMenu =>
                    {
                        string stringToCheckIfUserHasVotedBefore = $"{selectMenu.User.Id}";
                        if (poll.VoteHistory.Contains(stringToCheckIfUserHasVotedBefore))
                        {
                            selectMenu.RespondAsync("Cannot vote more than once", ephemeral: true);
                            return;
                        }
                        poll.Options.First(x => x.Title == selectMenu.Data.Values.First()).Votes++;
                        selectMenu.Message.ModifyAsync(props =>
                            props.Embed = GenerateEmbed().Build());
                        poll.VoteHistory.Add(stringToCheckIfUserHasVotedBefore);
                    
                        selectMenu.RespondAsync("Response submitted.", ephemeral:true);
                    }));
                
                    modal.RespondAsync(embed: GenerateEmbed().Build(), components:cb);
                }));
                cmd.RespondWithModalAsync(builtModal);
            }), guild);
        #endregion
    }
}