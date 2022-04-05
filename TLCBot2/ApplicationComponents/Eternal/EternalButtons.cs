using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.DataManagement.Temporary;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents.Eternal;

public static class EternalButtons
{
    public static ButtonBuilder EternalButton0 => new("button", "ETERNAL-BUTTON-0");
    public static ButtonBuilder EternalButton1 => new("Server Directory", "ETERNAL-BUTTON-1");
    public static ButtonBuilder EternalButton2 => new("Feedback", "ETERNAL-BUTTON-2");
    public static ButtonBuilder EternalButton5 => new("Command Catalogue", "ETERNAL-BUTTON-5");
    public static ButtonBuilder EternalButton6 => new("Role Catalogue", "ETERNAL-BUTTON-6");
    public static void OnExecute(SocketMessageComponent button)
    {
        const string remove = "ETERNAL-BUTTON-";
        int id = int.Parse(button.Data.CustomId.Remove(0, remove.Length));

        switch (id)
        {
            case 1:
                string generalChattingID = $"button-{Helper.RandomInt(0, 9999)}";
                string ArtSharingID = $"button-{Helper.RandomInt(0, 9999)}";
                string ImproveSupportID = $"button-{Helper.RandomInt(0, 9999)}";
                button.RespondAsync(components: FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                    .WithButton("Hangout", generalChattingID)
                    .WithButton("Share", ArtSharingID)
                    .WithButton("Improve & Support", ImproveSupportID)
                    , subButton =>
                {
                    if (subButton.Data.CustomId == generalChattingID)
                        subButton.RespondAsync(
                            "**--- Hangout ---**\n\n" +
                            "<#616111484505948160>```The first general chat. Use this channel to chat with other server " +
                            "members about anything that doesn't already have it's own channel.```\n" +
                            "<#644633097790292008>```The second general chat. This channel is identical to the first, " +
                            "but use this channel when the first chat is already occupied with a conversation, " +
                            "as to not disrupt the people talking in it.```\n" +
                            "<#616111528856256512>```The pictures and videos chat. Use this channel to post any type of media you " +
                            "want to share with people, even if it is not related to previous conversations.```\n" +
                            "<#632055973548261416>```The funny stuff chat. Use this channel to share content that you think is funny, " +
                            "such as memes.```\n" +
                            "<#755178792397307944>```The doodles only chat. Use this channel to talk with other server members but only " +
                            "using artwork that you have made, such as doodles that express your thoughts or feelings.```\n" +
                            "<#645807135187402782>```The daily question channel. Mods will send a question once a day in this channel. Answer " +
                            "the questions by replying in the threads that the mods will make, and try not to get too far from " +
                            "the topic of the original question.```\n" +
                            "<#618522076243951646>```The bot commands channel. This channel is for using or trying out bot commands.```"
                            , ephemeral: true);
                    else if (subButton.Data.CustomId == ArtSharingID)
                        subButton.RespondAsync(
                            "**--- Share ---**\n\n" +
                            "<#616111785698656266>```The art discussion chat. Use this channel to discuss with other server " +
                            "members about art related subjects or advice, that isn't necessarily posting art for show.```\n" +
                            "<#621524295847247903>```The studies and sketches chat. This channel is used when you want to, " +
                            "share your sketches with people, sketches which can be WIPs or just are just exercise.```\n" +
                            "<#616111891143720971>```The work in progress chat. Use this channel to post your WIPs or " +
                            "other unfinished art.```\n" +
                            "<#616111840547700746>```The finished art chat. Use this channel to share your art that you deem finished. " +
                            "The posted art must also be your own creation.```\n" +
                            "<#652276919684956180>```The starboard channel. When members react with a ⭐ emoji, and a total of 6 members do this, " +
                            "the bot will post the artwork in the starboard channel, as a sign of displaying something that people liked.```\n" +
                            "<#663647364870832149>```The DTIYS channel. Which stands for Draw This In Your Style, you can post renditions of " +
                            "people's OCs or other sorts of fan-art. You are also allowed to use this channel as a means to collaborate with other " +
                            "members on artworks, discuss art projects, and similar topics.```\n" +
                            "<#616112034836250624>```The inspiration channel. Use this channel to post artwork that inspires you or that " +
                            "gives you feelings of encouragement to draw. Do not post your own art here, and make sure to give credit to the artists.```\n" +
                            "<#656885921131528202>```The promotion channel. This channel is for promoting your social medias, websites, commissions, and the " +
                            "like. Feel free to post any kind of advertisement in here.```"
                            , ephemeral: true);
                    else if (subButton.Data.CustomId == ImproveSupportID)
                        subButton.RespondAsync(
                            "**--- Improve ---**\n\n" +
                            "<#616113663866306571>```The art critiquing chat. Use this channel to ask people to give criticism " +
                            "of your art and in what ways you can improve in the future.```\n" +
                            "<#616113768237236245>```The resources chat. This channel is used to post useful art resources, such as tutorials, " +
                            "websites, or anything you think would be beneficial for people to know exists.```\n" +
                            "<#666158488484839424>```The business chat. Use this channel to discuss business related subjects, such as marketing or " +
                            "advertising.```\n\n" +
                            "**--- Support ---**\n\n" +
                            "<#636579633592008705>```The good vibes channel. Use to share content that produces good vibes, such as you having a " +
                            "nice day or cute things that you enjoy looking at.```\n" +
                            "<#632055861308686338>```The venting channel. Use this channel to talk about anything that has been letting you down " +
                            "and to release any misunderstandings, anger, or sadness within you.```\n" +
                            "<#891484236769542154>```The tech support channel. For when you're having trouble with software or hardware and " +
                            "need a helping hand from the community to guide you through your technical problems.```"
                            , ephemeral: true);
                }, null)), ephemeral: true);
                break;
            case 2:
                button.RespondAsync(ephemeral: true, components: new FireMessageComponent(new ComponentBuilder()
                    .WithButton("Server Suggestion", $"0-button-{Helper.RandomInt(0, 9999)}")
                    .WithButton("Bug Report", $"1-button-{Helper.RandomInt(0, 9999)}")
                    .WithButton("QOTD Suggestion", $"2-button-{Helper.RandomInt(0, 9999)}")
                , subButton =>
                {
                    switch (subButton.Data.CustomId[0])
                    {
                        case '0':
                            subButton.RespondWithModalAsync(FireModal.CreateNew(new FireModal(new ModalBuilder()
                                    .WithTitle("Server Suggestion")
                                    .WithCustomId($"modal-{Helper.RandomInt(0, 1000)}")
                                    .AddTextInput("Suggestion", "textbox-0", TextInputStyle.Paragraph)
                                    .AddTextInput("Keep your suggestion anonymous", "textbox-1", TextInputStyle.Short,
                                        "Input 'y' for yes and 'n' for no", maxLength: 1),
                                modal =>
                                {
                                    var suggestion = modal.Data.Components.First().Value;
                                    var anonymous = modal.Data.Components.Last().Value == "y";
                                    string name = anonymous ? "[unknown]" : modal.User.Mention;
                                    var embed = new EmbedBuilder()
                                        .WithTitle("Server Suggestion Recieved")
                                        .WithColor(Color.Blue)
                                        .AddField("Sent From", name)
                                        .AddField("What They Suggest", suggestion);

                                    if (!anonymous) embed.WithAuthor(modal.User);
                                    var poll = new Poll(suggestion, new Poll.Option[]
                                    {
                                        new("Agree"),
                                        new("Disagree")
                                    });

                                    var fmc = new FireMessageComponent(new ComponentBuilder()
                                            .WithButton("Agree", $"0-button-{Helper.RandomInt(0, 9999)}")
                                            .WithButton("Disagree", $"1-button-{Helper.RandomInt(0, 9999)}")
                                            .WithButton("Show Votes", $"2-button-{Helper.RandomInt(0, 9999)}")
                                        , subSubButton =>
                                        {
                                            bool TryVote(string voteFor)
                                            {
                                                ulong userId = subSubButton.User.Id;
                                                if (poll.VoteHistory.Contains(userId))
                                                {
                                                    subSubButton.RespondAsync("Cannot vote more than once",
                                                        ephemeral: true);
                                                    return false;
                                                }

                                                poll.Options.First(x => x.Title == voteFor).Votes++;
                                                poll.VoteHistory.Add(userId);
                                                return true;
                                            }

                                            switch (subSubButton.Data.CustomId[0])
                                            {
                                                case '0':
                                                    if (!TryVote("Agree"))
                                                        return;
                                                    subSubButton.RespondAsync("Voted with `Agree`", ephemeral: true);
                                                    break;
                                                case '1':
                                                    if (!TryVote("Disagree"))
                                                        return;
                                                    subSubButton.RespondAsync("Voted with `Disagree`", ephemeral: true);
                                                    break;
                                                case '2':
                                                    if (!(Constants.Users.IsDev(subSubButton.User) ||
                                                          poll.VoteHistory.Contains(subSubButton.User.Id)))
                                                    {
                                                        subSubButton.RespondAsync(
                                                            "You must vote before viewing the results",
                                                            ephemeral: true);
                                                        return;
                                                    }

                                                    EmbedBuilder SubGenerateEmbed()
                                                    {
                                                        var subEmbed = new EmbedBuilder().WithTitle(suggestion)
                                                            .WithColor(Color.Blue);
                                                        int allVotes = poll.Options.Sum(opt => opt.Votes);
                                                        foreach (var option in poll.Options)
                                                        {
                                                            const int charCount = 20;
                                                            const char w = '#';
                                                            const char b = '-';
                                                            float votePercent =
                                                                (allVotes > 0
                                                                    ? (float) option.Votes / allVotes
                                                                    : option.Votes) * charCount;

                                                            string whiteBar =
                                                                $"{(votePercent > 0 ? new string(w, (int) votePercent) : "")}";
                                                            string blackBar =
                                                                $"{new string(b, (int) (charCount - votePercent))}";
                                                            string fullBar = $"{whiteBar}{blackBar}";
                                                            fullBar = fullBar.Length == charCount - 1
                                                                ? fullBar + b
                                                                : fullBar;
                                                            string spacing = new(' ',
                                                                allVotes.ToString().Length -
                                                                option.Votes.ToString().Length);
                                                            subEmbed.AddField(option.Title,
                                                                $"`{option.Votes + spacing} [{fullBar}]`");
                                                        }

                                                        subEmbed.WithFooter($"Total votes: {allVotes}");

                                                        if (!anonymous)
                                                            subEmbed.WithAuthor(modal.User);

                                                        if (Constants.Users.IsDev(subSubButton.User))
                                                        {
                                                            subEmbed.AddField("Mod Only",
                                                                $"Suggestor: {modal.User.Mention}");
                                                        }

                                                        return subEmbed;
                                                    }

                                                    subSubButton.RespondAsync(embed: SubGenerateEmbed().Build(),
                                                        ephemeral: true);
                                                    break;
                                            }
                                        }, null)
                                    {
                                        BirthDate = DateTime.Now.AddDays(7)
                                    }.Create();

                                    var msg = RuntimeConfig.ServerSuggestionsChannel.SendMessageAsync(embed: embed.Build(),
                                        components: fmc).Result;
                                    ((SocketTextChannel) msg.Channel).CreateThreadAsync($"Discussion", message: msg);
                                    modal.RespondAsync("Suggestion submitted", ephemeral: true);
                                })));
                            break;
                        case '1':

                            subButton.RespondWithModalAsync(FireModal.CreateNew(new FireModal(new ModalBuilder()
                                    .WithTitle("Bug Report")
                                    .WithCustomId($"modal-{Helper.RandomInt(0, 1000)}")
                                    .AddTextInput("The bug you encountered", "textbox-0", TextInputStyle.Paragraph),
                                modal =>
                                {
                                    RuntimeConfig.FeedbackReceptionChannel.SendMessageAsync(embed: new EmbedBuilder()
                                        .WithTitle("Bug Reported")
                                        .WithColor(Color.Red)
                                        .WithAuthor(modal.User)
                                        .AddField("Sent From", modal.User.Mention)
                                        .AddField("What They Reported", modal.Data.Components.First().Value)
                                        .Build());
                                    modal.RespondAsync("Bug Reported", ephemeral: true);
                                })));
                            break;
                        case '2':
                            subButton.RespondWithModalAsync(FireModal.CreateNew(new FireModal(new ModalBuilder()
                                    .WithTitle("QOTD Suggestion")
                                    .WithCustomId($"modal-{Helper.RandomInt(0, 1000)}")
                                    .AddTextInput("Suggest A Question", "textbox-0"),
                                modal =>
                                {
                                    var question = modal.Data.Components.First().Value!;
                                    modal.RespondAsync("Question Suggested", ephemeral: true);
                                    RuntimeConfig.FeedbackReceptionChannel.SendMessageAsync(embed: new EmbedBuilder()
                                        .WithTitle("QOTD Suggestion")
                                        .WithColor(Color.Blue)
                                        .WithAuthor(modal.User)
                                        .AddField("Sent From", modal.User.Mention)
                                        .AddField("What They Suggested", question)
                                        .Build(), components: new FireMessageComponent(new ComponentBuilder()
                                            .WithButton("Post to #QOTD", 
                                                $"0-button-{Helper.RandomInt(0, 9999)}")
                                            .WithButton("Edit and post to #QOTD",
                                                $"1-button-{Helper.RandomInt(0, 9999)}")
                                        , subSubButton =>
                                        {
                                            if (subSubButton.Data.CustomId[0] == '0')
                                            {
                                                var msg = RuntimeConfig.QOTDChannel
                                                    .SendMessageAsync(
                                                        $"{RuntimeConfig.QOTDRole.Mention} {question}\n\n" +
                                                        $"Thanks to {modal.User.Mention} for suggesting this question.")
                                                    .Result;
                                                Helper.DisableMessageComponents(subSubButton.Message);
                                                subSubButton.RespondAsync(
                                                    $"Posted question on {RuntimeConfig.QOTDChannel.Mention}\n\n{msg.GetJumpUrl()}",
                                                    ephemeral: true);
                                            }
                                            else
                                            {
                                                subSubButton.RespondWithModalAsync(new FireModal(new ModalBuilder()
                                                        .WithTitle("Edit message and post")
                                                        .WithCustomId($"modal-{Helper.RandomInt(0, 9999)}")
                                                        .AddTextInput("New messages content", "tb-0",
                                                            TextInputStyle.Paragraph, value: question),
                                                    subModal =>
                                                    {
                                                        var msg = RuntimeConfig.QOTDChannel
                                                            .SendMessageAsync(
                                                                $"{RuntimeConfig.QOTDRole.Mention} {subModal.Data.Components.First().Value}\n\n" +
                                                                $"Thanks to {modal.User.Mention} for suggesting this question.")
                                                            .Result;
                                                        subModal.RespondAsync(
                                                            $"Posted question on {RuntimeConfig.QOTDChannel.Mention}\n\n{msg.GetJumpUrl()}",
                                                            ephemeral: true);
                                                        Helper.DisableMessageComponents(subSubButton.Message);
                                                    }).Create());
                                            }
                                        }, null).Create());
                                })));
                    break;
                    }
                }, null).Create());
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                button.RespondAsync("The documentation of all the commands can be found here: " +
                                    "https://www.github.com/firebreak-2/tlcbot2/wiki/Commands", ephemeral: true);
                break;
            case 6:
                button.RespondAsync(components: new FireMessageComponent(new ComponentBuilder()
                    .WithButton("Vanity and Access roles", $"0-button-{Helper.RandomInt(0,9999)}")
                    .WithButton("Ping, Special, and Legacy roles", $"1-button-{Helper.RandomInt(0,9999)}")
                , subButton =>
                {
                    switch (subButton.Data.CustomId[0])
                    {
                        case '0':
                            subButton.RespondAsync(
                                "**--- Vanity Roles ---**\n\n" +
                                "```Color Roles: Any role beginning with the prefix [Color] is a color role, and solely exists " +
                                "to allow users to change their username color on the server```\n" +
                                "```Art Specialty roles: There are 2 roles in this category. The Digital Artist role and the " +
                                "Traditional Artist role. The purpose of these roles is to let others' know what kind of art " +
                                "you do. You may pick either or both roles, and they give a grey-ish color as a sub-feature```\n" +
                                "```Pronoun Roles: Any role beginning with the prefix [Pronoun] is a pronoun role. These roles exist " +
                                "so that members can easily find out what a specific member prefers being called by```\n" +
                                "**--- Access Roles ---**\n\n" +
                                "```18+ Roles: These roles give you access to the NSFW channels. Do not use these unless you are over 18. " +
                                "The NSFW access role give's access to all NSFW channels, while the figure drawing role gives access only to " +
                                "the #artistic-nudity channel```\n" +
                                "```Bot Fun Roles: These are any role that has the prefix [Bot Fun], they give access to the channel mentioned " +
                                "their names respectively.```"
                                , ephemeral: true);
                            break;
                        case '1':
                            subButton.RespondAsync(
                                "**--- Ping Roles ---**\n\n" +
                                "```Permanent Ping Roles: These are one's which you will always see around and are always able to use " +
                                "them. Their purpose is to be pinged upon their respective event triggering, such as a QOTD being posted```\n" +
                                "```Event Ping Roles: These roles are event-specific and they will generally be deleted when the event is done. " +
                                "They are pinged when the event starts, concludes, or there is an announcement for the specified event```\n" +
                                "**--- Special Roles ---**\n\n" +
                                "```Boostin Buddies: This role is given to server boosters as a sign of appreciation, it also give's access to a " +
                                "secret channel```\n" +
                                "```Mod Role: This role is for the server moderators. The role gives them access to the server's mod-only chats " +
                                "and gives them administrative power in the server. The only way to acquire this role is through the server owner " +
                                "directly giving it to you, which generally happens after applying for the role during TLC mod applications```\n" +
                                "```Developer Role: This role is given to the server's custom bot developers, and give's them access " +
                                "to the developer only channels that are used to manage the bot or to print test outputs from the bot```\n" +
                                "**--- Legacy Roles ---**\n\n" +
                                "```Server Helper and Art Helper: These roles used to be self-assigned for people who were willing to be pinged " +
                                "when someone needed help with their art or with the server. Now they have been discontinued and have no use or color " +
                                "and can no longer be acquired```"
                                , ephemeral: true);
                            break;
                    }
                }, null).Create(), ephemeral: true);
                break;
        }
    }
}