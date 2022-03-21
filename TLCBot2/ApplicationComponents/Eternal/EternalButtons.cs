using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents.Eternal;

public static class EternalButtons
{
    public static ButtonBuilder EternalButton0 => new("button", "ETERNAL-BUTTON-0");
    public static ButtonBuilder EternalButton1 => new("Server Directory", "ETERNAL-BUTTON-1");
    public static ButtonBuilder EternalButton2 => new("Feedback", "ETERNAL-BUTTON-2");
    public static ButtonBuilder EternalButton3 => new("Bug Report", "ETERNAL-BUTTON-3");
    public static ButtonBuilder EternalButton4 => new("QOTD Suggestion", "ETERNAL-BUTTON-4");
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
                button.RespondWithModalAsync(FireModal.CreateNew(new FireModal(new ModalBuilder()
                    .WithTitle("Feedback Submission")
                    .WithCustomId($"modal-{Helper.RandomInt(0, 1000)}")
                    .AddTextInput("Feedback", "textbox-0", TextInputStyle.Paragraph, "The server provides no pizza :("), 
                    modal =>
                {
                    RuntimeConfig.FeedbackReceptionChannel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithTitle("Feedback Recieved")
                        .WithColor(Color.Gold)
                        .WithAuthor(modal.User)
                        .AddField("Sent From", modal.User.Mention)
                        .AddField("What They Submitted", modal.Data.Components.First().Value)
                        .Build());
                    modal.RespondAsync("Feedback submitted", ephemeral: true);
                })));
                break;
            case 3:
                button.RespondWithModalAsync(FireModal.CreateNew(new FireModal(new ModalBuilder()
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
            case 4:
                button.RespondWithModalAsync(FireModal.CreateNew(new FireModal(new ModalBuilder()
                    .WithTitle("QOTD Suggestion")
                    .WithCustomId($"modal-{Helper.RandomInt(0, 1000)}")
                    .AddTextInput("Suggest A Question", "textbox-0"),
                    modal =>
                    {
                        var question = modal.Data.Components.First().Value;
                    RuntimeConfig.FeedbackReceptionChannel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithTitle("QOTD Suggestion")
                        .WithColor(Color.Blue)
                        .WithAuthor(modal.User)
                        .AddField("Sent From", modal.User.Mention)
                        .AddField("What They Suggested", question)
                        .Build(), components: new FireMessageComponent(new ComponentBuilder()
                        .WithButton("Post to #QOTD", $"button-0-{Helper.RandomInt(0, 9999)}")
                        .WithButton("Edit and post to #QOTD", $"button-1-{Helper.RandomInt(0, 9999)}")
                        , qotdButton =>
                    {

                        if (qotdButton.Data.CustomId.StartsWith("button-0"))
                        {
                            var msg = RuntimeConfig.QOTDChannel
                                .SendMessageAsync($"{RuntimeConfig.QOTDRole.Mention} {question}\n\n" +
                                                  $"Thanks to {modal.User.Mention} for suggesting this question.").Result;
                            
                            qotdButton.RespondAsync(
                                $"Posted question on {RuntimeConfig.QOTDChannel.Mention}\n\n{msg.GetJumpUrl()}",
                                ephemeral: true);

                            qotdButton.Message.ModifyAsync(props =>
                            {
                                props.Components = new ComponentBuilder()
                                    .WithButton("Post to #QOTD", "no-use", ButtonStyle.Secondary, disabled: true)
                                    .Build();
                            });
                        }
                        else
                        {
                            qotdButton.RespondWithModalAsync(new FireModal(
                                new ModalBuilder("Post question to #QOTD", $"modal-{Helper.RandomInt(0, 9999)}")
                                    .AddTextInput("Edited Question", "tb-0", TextInputStyle.Paragraph, value: question),
                                modal =>
                                {
                                    var msg = RuntimeConfig.QOTDChannel
                                        .SendMessageAsync($"{RuntimeConfig.QOTDRole.Mention} {question}\n\n" +
                                                          $"Thanks to {modal.User.Mention} for suggesting this question.").Result;

                                    modal.RespondAsync(
                                        $"Posted question on {RuntimeConfig.QOTDChannel.Mention}\n\n{msg.GetJumpUrl()}",
                                        ephemeral: true);

                                    qotdButton.Message.ModifyAsync(props =>
                                    {
                                        props.Components = new ComponentBuilder()
                                            .WithButton("Post to #QOTD", "no-use", ButtonStyle.Secondary, disabled: true)
                                            .WithButton("Edit and post to #QOTD", "no-use", ButtonStyle.Secondary, disabled: true)
                                            .Build();
                                    });
                                }).Create());
                        }

                    }, null){BirthDate = DateTime.Now.AddDays(7)}.Create());
                    modal.RespondAsync("Question Suggested", ephemeral: true);
                })));
                break;
            case 5:
                button.RespondAsync("fire did not yet implement this button, it exists tho", ephemeral: true);
                break;
            case 6:
                button.RespondAsync("fire did not yet implement this button, it exists tho", ephemeral: true);
                break;
        }
    }
}