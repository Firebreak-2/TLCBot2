using Discord;
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
                        foreach (var option in poll.Options)
                        {
                            embed.AddField(option.Title, option.Votes);
                        }

                        if (!anonymous)
                            embed.WithAuthor(cmd.User);
    
                        return embed;
                    }
                
                    var cb = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                        .WithSelectMenu(poll.CustomID, poll.Options.Select(option => new SelectMenuOptionBuilder()
                            .WithLabel(option.Title)
                            // .WithDescription(option.Votes.ToString())
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
            }, true), guild);
        #endregion
        
        #region Color Photo Command
        // await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
        //         .WithName("color-photo")
        //         .WithDescription("Displays the most prominent colors in an image")
        //         .AddOption("image", ApplicationCommandOptionType.Attachment, "The image to be checked", true),
        //     cmd =>
        //     {
        //         var attachment = (Attachment) cmd.Data.Options.First().Value;
        //         if (!attachment.ContentType.StartsWith("image"))
        //         {
        //             cmd.RespondAsync("File uploaded must be an image.", ephemeral: true);
        //         }
        //         Console.WriteLine($"Filename    | {attachment.Filename}");
        //         Console.WriteLine($"Description | {attachment.Description}");
        //         Console.WriteLine($"ContentType | {attachment.ContentType}");
        //         Console.WriteLine($"ProxyUrl    | {attachment.ProxyUrl}");
        //         Console.WriteLine($"Id          | {attachment.Id}");
        //         Console.WriteLine($"Size        | {attachment.Width}x{attachment.Height}");
        //
        //         cmd.RespondAsync("a", ephemeral: true);
        //     }), guild);
        #endregion
    }
}