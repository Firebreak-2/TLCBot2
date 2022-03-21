using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.DataManagement;
using TLCBot2.DataManagement.Temporary;
using TLCBot2.Utilities;
using Color = Discord.Color;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.ApplicationComponents.Commands.SlashCommands
{
    public static class CommercialSlashCommands
    {
        public static async Task Initialize()
        {
            var guild = Constants.Guilds.Lares!;
            const bool devOnly = false;

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
                }, devOnly), guild);
            #endregion

            #region Poll Command
            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                .WithName("poll")
                .WithDescription("Conducts a poll")
                .AddOption("title", ApplicationCommandOptionType.String, "The title(question) of the poll", true)
                .AddOption("options", ApplicationCommandOptionType.Integer, "The amount of options (2-5)", true, minValue:2, maxValue:5)
                .AddOption("anonymous", ApplicationCommandOptionType.Boolean, "Whether you want to make it public that you made this poll")
                .AddOption("has-other", ApplicationCommandOptionType.Boolean, "Whether to include an \"other\" option")
                ,
                cmd =>
                {
                    string title = (string) cmd.Data.Options.First(x => x.Name == "title").Value;
                    long optionCount = (long) cmd.Data.Options.First(x => x.Name == "options").Value;
                    bool anonymous = 
                        cmd.Data.Options.All(x => x.Name != "anonymous") 
                        || (bool)cmd.Data.Options.First(x => x.Name == "anonymous").Value;
                    bool hasOther = 
                        cmd.Data.Options.Any(x => x.Name == "has-other") 
                        && (bool)cmd.Data.Options.First(x => x.Name == "has-other").Value;
                    
                    var modalBuilder = new ModalBuilder()
                        .WithTitle("Poll Options")
                        .WithCustomId($"modal-{Helper.RandomInt(0, 1000)}");
                    
                    for (int i = 0; i < optionCount; i++)
                    {
                        modalBuilder.AddTextInput($"Option {i+1}", $"{i}");
                    }
                    
                    var builtModal = FireModal.CreateNew(new FireModal(modalBuilder, modal =>
                    {
                        var options = new Poll.Option[optionCount + (hasOther ? 1 : 0)];
                        foreach (var item in modal.Data.Components)
                        {
                            int index = int.Parse(item.CustomId);
                            options[index] = new Poll.Option(item.Value);
                        }

                        if (hasOther) options[^1] = new Poll.Option("Other");
                        
                        var poll = new Poll(title, $"poll-{Helper.RandomInt(0, 1000)}", options);
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
                                embed.AddField(option.Title, $"`{option.Votes + spacing} [{fullBar}]`"); 
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
                    
                        var msg = modal.Channel.SendMessageAsync(embed: GenerateEmbed().Build(), components:cb).Result;
                        modal.RespondAsync();
                        if (hasOther)
                        {
                            ((SocketTextChannel) msg.Channel)
                                .CreateThreadAsync(
                                    "Other Answers", message: msg);
                        }
                    }));
                    cmd.RespondWithModalAsync(builtModal);
                }, devOnly), guild);
            #endregion
        
            #region Color Command
                await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                        .WithName("color")
                        .WithDescription("Displays the specified color.")
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("hex")
                            .WithDescription("The hexadecimal value to interpret the color")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption("value", ApplicationCommandOptionType.String, "the hex value", true))
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("rgb")
                            .WithDescription("The RGB value to interpret the color")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("red")
                                .WithDescription("The red color value")
                                .WithType(ApplicationCommandOptionType.Integer)
                                .WithRequired(true)
                                .WithMinValue(0)
                                .WithMaxValue(255))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("green")
                                .WithDescription("The green color value")
                                .WithType(ApplicationCommandOptionType.Integer)
                                .WithRequired(true)
                                .WithMinValue(0)
                                .WithMaxValue(255))
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("blue")
                                .WithDescription("The blue color value")
                                .WithType(ApplicationCommandOptionType.Integer)
                                .WithRequired(true)
                                .WithMinValue(0)
                                .WithMaxValue(255)
                            )),
                        // .AddOption("color-hex", ApplicationCommandOptionType.String, "the color to show", true),
                    cmd =>
                    {
                        var colorType = (SocketSlashCommandDataOption)cmd.Data.Options.First();
                
                        Argb32 color;
                        switch (colorType.Name)
                        {
                            case "hex":
                                string colorHex = (string) colorType.Options.First().Value;
                                color = Helper.HexCodeToColor(colorHex).DiscordColorToArgb32();
                                break;
                            case "rgb":
                                byte colorR = Convert.ToByte((long) colorType.Options.ToArray()[0].Value);
                                byte colorG = Convert.ToByte((long) colorType.Options.ToArray()[1].Value);
                                byte colorB = Convert.ToByte((long) colorType.Options.ToArray()[2].Value);
                                color = new Color(colorR, colorG, colorB).DiscordColorToArgb32();
                                break;
                            default:
                                throw new Exception("how??!?!");
                        }
                        var inverseColor = color.Invert();
                    
                        const string endName = "TLC_Watermark.png";
                        string path = $"{Program.FileAssetsPath}/{endName}";
                        using Image<Argb32> image = Image.Load<Argb32>(path);
    
                        var magenta = new Argb32(255, 0, 255);
                        image.FillColor((_, _, pixel) => pixel != magenta
                            ? color
                            : inverseColor);
                
                        using var stream = image.ToStream();
                        string text = $"`{color}`\n`{color.Argb32ToDiscordColor()}`";
                    
                        var embedBuilder = new EmbedBuilder()
                            .WithColor(color.Argb32ToDiscordColor())
                            .WithImageUrl(Helper.GetFileUrl(stream, null, text))
                            .WithTitle(text);
                    
                        cmd.RespondAsync(embed: embedBuilder.Build());
                }, devOnly), guild);
            #endregion
            
            #region Random Color Command
                await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                        .WithName("random-color")
                        .WithDescription("Displays a random color."),
                    cmd =>
                    {
                        Embed GetColorEmbed()
                        {
                            var color = new Color(Helper.RandomInt(0, 255),Helper.RandomInt(0, 255),Helper.RandomInt(0, 255))
                                .DiscordColorToArgb32();
                            var inverseColor = color.Invert();
                    
                            const string endName = "TLC_Watermark.png";
                            string path = Program.FileAssetsPath + '/' + endName;
                            using Image<Argb32> image = Image.Load<Argb32>(path);
    
                            var magenta = new Argb32(255, 0, 255);
                            image.FillColor((_, _, pixel) => pixel != magenta
                                ? color
                                : inverseColor);
                
                            using var stream = image.ToStream();
                            string text = $"`{color}`\n`{color.Argb32ToDiscordColor()}`";
                    
                            return new EmbedBuilder()
                                .WithColor(color.Argb32ToDiscordColor())
                                .WithImageUrl(Helper.GetFileUrl(stream, null, text))
                                .WithTitle(text)
                                .Build();
                        }
                        var embedBuilder = GetColorEmbed();

                        var fmc = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                            .WithButton("Reroll", $"reroll-randcol-{Helper.RandomInt(0, 1000)}"), button =>
                        {
                            button.Message.ModifyAsync(props => props.Embed = GetColorEmbed());
                            button.RespondAsync();
                        }, null));
                    
                        cmd.RespondAsync(embed: embedBuilder, components:fmc); 
                }, devOnly), guild);
            #endregion
            
            #region Scheme Command
                await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                        .WithName("scheme")
                        .WithDescription("Generate a color scheme to be used with a base color.")
                        .AddOption("base-color", ApplicationCommandOptionType.String, "The color to base the scheme off of.", true),
                    cmd =>
                    {
                        Embed GetSchemeEmbed()
                        {
                            string hexcode = (string) cmd.Data.Options.First()!;
                            var color = Helper.HexCodeToColor(hexcode);

                            using var client = new WebClient();
                            string result =
                                client.DownloadString(
                                    $"https://www.thecolorapi.com/scheme?hex={color.ToString().Remove(0, 1)}");

                            var matches = Regex.Matches(result, "(?<=\"hex\":{\"value\":\"#).{6}(?=\")");
                            var colors = matches.Select(x => Helper.HexCodeToColor(x.Value).DiscordColorToArgb32())
                                .ToArray();

                            string outp = string.Join("\n", colors.Select(col => $"{col.Argb32ToDiscordColor()}"));

                            using var image = new Image<Argb32>(100, 250);
                            image.FillColor((_, y) => colors[y / 50]);
                            string url = Helper.GetFileUrl(image.ToStream(), null,
                                $"```\n{outp}\n```");

                            return new EmbedBuilder()
                                .WithTitle($"Scheme for {cmd.User.Username}")
                                .WithImageUrl(url)
                                .WithColor(color)
                                .WithFooter(outp)
                                .Build();
                        }
                        cmd.RespondAsync(embed:GetSchemeEmbed());
                }, devOnly), guild);
            #endregion
            
            #region Bingo Command
            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                    .WithName("bingo")
                    .WithDescription("Generates a bingo board to draw from. More info on help command")
                    .AddOption("tile-count", ApplicationCommandOptionType.Integer, "the grid size",minValue:3,maxValue:11),
                cmd =>
                {
                    Embed GetBingoEmbed()
                    {
                        using var image = new Image<Argb32>(1000, 1000);
                        int tilesPerRow = cmd.Data.Options.Count > 0
                            ? Convert.ToInt32(cmd.Data.Options.First().Value)
                            : 5;
                        var bingoPrompts =
                            File.ReadAllLines($"{Program.FileAssetsPath}/bingoPrompts.cfg")
                                .Select(x => x.Replace(' ', '\n')).ToHashSet();

                        const int gridLineWidth = 5;
                        int tileWidth = image.Width / tilesPerRow;
                        int tileHeight = image.Height / tilesPerRow;

                        image.FillColor((x, y) =>
                        {
                            for (int i = 0; i < gridLineWidth; i++)
                            {
                                for (int j = 0; j < tilesPerRow; j++)
                                {
                                    int val = i + image.Width / tilesPerRow * j;
                                    if (val <= gridLineWidth) continue;

                                    if (x == val || y == val) return new Argb32(0, 0, 0);
                                }
                            }

                            return new Argb32(255, 255, 255);
                        });

                        var centerPos = Point.Empty;
                        for (int y = 0; y < tilesPerRow; y++)
                        {
                            for (int x = 0; x < tilesPerRow; x++)
                            {
                                int yPos = y * image.Height / tilesPerRow;
                                int xPos = x * image.Width / tilesPerRow;

                                if (x == y && x == tilesPerRow / 2)
                                {
                                    centerPos = new Point(xPos, yPos);
                                    continue;
                                }

                                string prompt = bingoPrompts.RandomChoice();
                                bingoPrompts.Remove(prompt);
                                var font = SystemFonts.CreateFont("Arial", prompt.Length > 8 ? 35 : 42);
                                var options = new TextOptions(font)
                                {
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Origin = new PointF(xPos + tileWidth / 2, yPos + tileHeight / 2),
                                    WordBreaking = WordBreaking.BreakAll,
                                    WrappingLength = tileWidth
                                };
                                image.Mutate(img =>
                                    img.DrawText(
                                        options,
                                        prompt,
                                        SixLabors.ImageSharp.Color.Black
                                    ));
                            }
                        }

                        using var imgToBeDrawn = Image.Load<Argb32>($"{Program.FileAssetsPath}/TLC_Logo.png");
                        imgToBeDrawn.Mutate(img =>
                            img.Resize(tileWidth, tileHeight));
                        image.Mutate(img =>
                            img.DrawImage(imgToBeDrawn, centerPos, 1));

                        string url = Helper.GetFileUrl(image.ToStream());
                        return new EmbedBuilder()
                            .WithTitle($"TLC bingo card for {cmd.User.Username}")
                            .WithDescription(
                                "Draw an image that would score a bingo on the following sheet. Don't forget to shout bingo and share your finished drawing!")
                            .WithImageUrl(url)
                            .WithColor(Color.Blue)
                            .Build();
                    }
                    
                    var fmc = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                        .WithButton("Reroll", $"reroll-bingo-{Helper.RandomInt(0, 1000)}"), button =>
                    {
                        button.Message.ModifyAsync(props => props.Embed = GetBingoEmbed());
                        button.RespondAsync();
                    }, null));

                    cmd.RespondAsync(embed:GetBingoEmbed(),components:fmc);
                    }, devOnly), guild);
        #endregion
        
            #region Random Prompt Command
            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                .WithName("prompt")
                .WithDescription("Generates a random prompt for you to draw!"), cmd =>
            {
                string prompt;
            
                string[] promptCharprops =    File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/charProps.cfg");
                string[] promptChars =        File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/chars.cfg");
                string[] promptColors =       File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/colors.cfg");
                string[] promptScenery =      File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/scenery.cfg");
                string[] promptSceneryprops = File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/sceneryProps.cfg");

                Embed GetRandomPromptEmbed()
                {

                    if (Helper.RandomInt(1, 20) == 1) // 1/20 chance to omit charProp
                        prompt =
                            $"{promptChars.RandomChoice()} {Regex.Replace(promptScenery.RandomChoice(), @"{.+?}", promptSceneryprops.RandomChoice())}";
                    else
                        prompt =
                            $"{promptChars.RandomChoice()} {promptCharprops.RandomChoice().NamedFormat("color", promptColors.RandomChoice())} {promptScenery.RandomChoice().NamedFormat("sceneryprop", promptSceneryprops.RandomChoice())}";

                    return new EmbedBuilder()
                        .WithTitle($"Art prompt for {cmd.User.Username}")
                        .WithDescription($"{prompt[0].ToString().ToUpper()}{prompt[1..]}.")
                        .WithColor(Color.Blue)
                        .Build();
                }
                
                var fmc = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                    .WithButton("Reroll", $"reroll-prompt-{Helper.RandomInt(0, 1000)}"), button =>
                {
                    button.Message.ModifyAsync(props => props.Embed = GetRandomPromptEmbed());
                    button.RespondAsync();
                }, null));

                cmd.RespondAsync(embed:GetRandomPromptEmbed(),components:fmc);
            }, devOnly), guild);
            #endregion
        
            #region Ping Command
            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                    .WithName("ping")
                    .WithDescription("Responds with \"pong!\" to indicate that the bot is online."), 
                cmd => cmd.RespondAsync("pong!"), devOnly), guild);
            #endregion
        
            #region User Info Command
            await FireSlashCommand.CreateNew(new(new SlashCommandBuilder()
                    .WithName("user-info")
                    .WithDescription("Peek into the user's details!")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to check the details of."),
                cmd =>
                {
                    var user = cmd.Channel.GetGuild().GetUser(((SocketUser?)(cmd.Data.Options.Any() ? cmd.Data.Options.First().Value : null) ?? cmd.User).Id);

                    int cookies = 0;
                    if (CookieManager.GetUser(user.Id, out var entry))
                        cookies = entry.Cookies;

                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithAuthor(user)
                        .AddField("Server Join Date", $"<t:{user.JoinedAt!.Value.ToUnixTimeSeconds()}>")
                        .AddField("Account Creation Date", $"<t:{user.CreatedAt.ToUnixTimeSeconds()}>")
                        .AddField("Cookies", cookies)
                        .AddField("Is Cookie Banned", entry?.IsBanned ?? false ? "Yes" : "No")
                        .AddField("User ID", user.Id);
                    cmd.RespondAsync(embed:embed.Build(), ephemeral: true);
                }, devOnly), guild);
            #endregion
        
            #region Cookies Command
            await FireSlashCommand.CreateNew(new(new SlashCommandBuilder()
                    .WithName("cookies")
                    .WithDescription("Check the amount of 🍪 that you own!")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to check the cookies of."),
                cmd =>
                {
                    var user = (SocketUser?)(cmd.Data.Options.Any() ? cmd.Data.Options.First().Value : null) ?? cmd.User;

                    int cookies = 0;
                    if (CookieManager.GetUser(user.Id, out var entry))
                        cookies = entry.Cookies;

                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle($"{user.Username}'s balance: {cookies}");
                
                    cmd.RespondAsync(embed:embed.Build());
                }, devOnly), guild);
            #endregion
        
            #region Cookie Leaderboard Command
            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                    .WithName("leaderboard")
                    .WithDescription("Shows the current 🍪 leaderboard."),
                cmd =>
                {
                    string output = "No users registered in leaderboard";
                    if (CookieManager.CookieUsers.Any())
                        output = string.Join("\n", CookieManager.CookieUsers
                            .Where((_, i) => i < 10).Select((user, i) =>
                        {
                            string rankingPrefix = i switch
                            {
                                0 => "🥇",
                                1 => "🥈",
                                2 => "🥉",
                                _ => $"#{i+1}"
                            };
                            string highlightUnderline = user.UserID == cmd.User.Id ? "__" : "";
                            string banDash = user.IsBanned ? "~~" : "";
                            string userMention = $"<@!{user.UserID}>";
                            return $"{highlightUnderline}{rankingPrefix}: **{user.Cookies}**🍪  {banDash}{userMention}{banDash}{highlightUnderline}";
                        }));
                    
                    var embed = new EmbedBuilder()
                        .WithTitle("TLC 🍪 Leaderboard")
                        .WithColor(Color.Blue)
                        .WithDescription(output)
                        .WithCurrentTimestamp();
                    
                    cmd.RespondAsync(embed:embed.Build());
                }, devOnly), guild);
            #endregion
        
            #region Social Media Command
            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                .WithName("social-media")
                .WithDescription("Displays a person's linked social media accounts")
                .AddOption("user", ApplicationCommandOptionType.User, "The user to check the socials of"),
                cmd =>
                {
                    SocketUser user =  (cmd.Data.Options.Count == 1 ? (SocketUser) cmd.Data.Options.First().Value : cmd.User);
                    if (!SocialMediaManager.GetUser(user.Id, out var entry))
                    {
                        cmd.RespondAsync($"{user.Username} has not linked any of their social media profiles.");
                        return;
                    }
       
                    var embed = new EmbedBuilder()
                        .WithTitle($"{user.Username}'s Social Media Profiles")
                        .WithColor(Color.Blue);
       
                    bool IsValidLink(string link)
                    {
                        return 
                            link != SocialMediaManager.SocialMediaUserEntry.NoLink 
                            && link.Any() 
                            && Helper.CheckStringIsLink(link);
                    }
                    string GetProfileName(string link)
                    {
                        link = link.EndsWith("/") ? link[..^1] : link;
                        return Regex.Match(link, @"(?<=\/)[^\/]+(?=\/)?$").Value;
                    }
       
                    string AsHyperlink(string link)
                    {
                        return $"[{GetProfileName(link)}]({link})";
                    }
       
                    if (IsValidLink(entry.Twitter        )) embed.AddField("Twitter"         , AsHyperlink(entry.Twitter        ));
                    if (IsValidLink(entry.Youtube        )) embed.AddField("YouTube"         , AsHyperlink(entry.Youtube        ));
                    if (IsValidLink(entry.Twitch         )) embed.AddField("Twitch"          , AsHyperlink(entry.Twitch         ));
                    if (IsValidLink(entry.TikTok         )) embed.AddField("TikTok"          , AsHyperlink(entry.TikTok         ));
                    if (IsValidLink(entry.Instagram      )) embed.AddField("Instagram"       , AsHyperlink(entry.Instagram      ));
                    if (IsValidLink(entry.DeviantArt     )) embed.AddField("DeviantArt"      , AsHyperlink(entry.DeviantArt     ));
                    if (IsValidLink(entry.ArtStation     )) embed.AddField("ArtStation"      , AsHyperlink(entry.ArtStation     ));
                    if (IsValidLink(entry.Reddit         )) embed.AddField("Reddit"          , AsHyperlink(entry.Reddit         ));
                    if (IsValidLink(entry.Steam          )) embed.AddField("Steam"           , AsHyperlink(entry.Steam          ));
                    if (IsValidLink(entry.GitHub         )) embed.AddField("GitHub"          , AsHyperlink(entry.GitHub         ));
                    if (IsValidLink(entry.PersonalWebsite)) embed.AddField("Personal Website", AsHyperlink(entry.PersonalWebsite));
       
                    if (!embed.Fields.Any())
                    {
                        cmd.RespondAsync($"{user.Username} has not linked any of their social media profiles.");
                        return;
                    }
       
                    cmd.RespondAsync(embed: embed.Build());
                }, devOnly), guild);
            #endregion
            
            #region Link Social Media Profile Command
            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                .WithName("link")
                .WithDescription("Links your discord account with a social media profile")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("platform")
                    .WithDescription("The social media platform to be linked to")
                    .WithType(ApplicationCommandOptionType.String)
                    .AddChoice("YouTube", "youtube")
                    .AddChoice("Twitter", "twitter")
                    .AddChoice("DeviantArt", "deviantart")
                    .AddChoice("Instagram", "instagram")
                    .AddChoice("GitHub", "github")
                    .AddChoice("Steam", "steamcommunity")
                    .AddChoice("Reddit", "reddit")
                    .AddChoice("ArtStation", "artstation")
                    .AddChoice("TikTok", "tiktok")
                    .AddChoice("Twitch", "twitch")
                    .AddChoice("Personal Website", "website")
                )
                .AddOption("profile-link", ApplicationCommandOptionType.String, "The **link** to your social media profile", true),
                cmd =>
                {
                    string profLink = (string) cmd.Data.Options.First(x => x.Name == "profile-link").Value;
                    string platform;
                    if (cmd.Data.Options.Count == 1)
                    {
                        var matcher = new Regex(@"https?:\/\/(?:www\.)?(.+)\.(?:tv|com)\/?(?:.+)?");
                        var match = matcher.Match(profLink);
                        if (!match.Success)
                        {
                            cmd.RespondAsync("Input is not a link", ephemeral: true);
                            return;
                        }
                        platform = matcher.Replace(profLink, "$1").ToLower();
       
                        switch (platform)
                        {
                            case "youtube":
                            case "twitter":
                            case "deviantart":
                            case "instagram":
                            case "github":
                            case "steamcommunity":
                            case "reddit":
                            case "artstation":
                            case "tiktok":
                            case "twitch":
                                break;
                            default:
                                platform = "website";
                                break;
                        }
                    }
                    else
                    {
                        platform = (string) cmd.Data.Options.First(x => x.Name == "platform").Value;
                    }
       
                    ulong userId = cmd.User.Id;
                    
                    switch (platform)
                    {
                        case "youtube":
                            SocialMediaManager.AddOrModifyUser(userId, Youtube: profLink);
                            cmd.RespondAsync($"Linked your YouTube profile with the link {profLink}", ephemeral:true);
                            break;
                        case "twitter":
                            SocialMediaManager.AddOrModifyUser(userId, Twitter: profLink);
                            cmd.RespondAsync($"Linked your Twitter profile with the link {profLink}", ephemeral:true);
                            break;
                        case "deviantart":
                            SocialMediaManager.AddOrModifyUser(userId, DeviantArt: profLink);
                            cmd.RespondAsync($"Linked your DeviantArt profile with the link {profLink}", ephemeral:true);
                            break;
                        case "instagram":
                            SocialMediaManager.AddOrModifyUser(userId, Instagram: profLink);
                            cmd.RespondAsync($"Linked your Instagram profile with the link {profLink}", ephemeral:true);
                            break;
                        case "github":
                            SocialMediaManager.AddOrModifyUser(userId, GitHub: profLink);
                            cmd.RespondAsync($"Linked your GitHub profile with the link {profLink}", ephemeral:true);
                            break;
                        case "steamcommunity":
                            SocialMediaManager.AddOrModifyUser(userId, Steam: profLink);
                            cmd.RespondAsync($"Linked your Steam profile with the link {profLink}", ephemeral:true);
                            break;
                        case "reddit":
                            SocialMediaManager.AddOrModifyUser(userId, Reddit: profLink);
                            cmd.RespondAsync($"Linked your Reddit profile with the link {profLink}", ephemeral:true);
                            break;
                        case "artstation":
                            SocialMediaManager.AddOrModifyUser(userId, ArtStation: profLink);
                            cmd.RespondAsync($"Linked your ArtStation profile with the link {profLink}", ephemeral:true);
                            break;
                        case "tiktok":
                            SocialMediaManager.AddOrModifyUser(userId, TikTok: profLink);
                            cmd.RespondAsync($"Linked your TikTok profile with the link {profLink}", ephemeral:true);
                            break;
                        case "twitch":
                            SocialMediaManager.AddOrModifyUser(userId, Twitch: profLink);
                            cmd.RespondAsync($"Linked your Twitch profile with the link {profLink}", ephemeral:true);
                            break;
                        case "website":
                            SocialMediaManager.AddOrModifyUser(userId, PersonalWebsite: profLink);
                            cmd.RespondAsync($"Linked your Personal Website with the link {profLink}", ephemeral:true);
                            break;
                        default:
                            cmd.RespondAsync("This social media platform is not supported :(", ephemeral:true);
                            break;
                    }
                }, devOnly), guild);
            #endregion
       
            #region  Unlink Social Media Profile

            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                .WithName("unlink")
                .WithDescription("Unlinks the selected social media profiles of yours"),
            cmd =>
            {
                cmd.RespondAsync(components: new FireMessageComponent(
                    new ComponentBuilder()
                        .WithSelectMenu($"select-menu-{Helper.RandomInt(0, 1000)}", new List<SelectMenuOptionBuilder>
                        {
                            new("YouTube", "YouTube"),
                            new("Twitter", "Twitter"),
                            new("Deviantart", "Deviantart"),
                            new("Instagram", "Instagram"),
                            new("GitHub", "GitHub"),
                            new("Steam", "Steam"),
                            new("Reddit", "Reddit"),
                            new("ArtStation", "ArtStation"),
                            new("TikTok", "TikTok"),
                            new("Twitch", "Twitch"),
                            new("Personal Website", "Personal Website")
                        }, maxValues:11), null, selectMenu =>
                    {
                        SocialMediaManager.GetUser(cmd.User.Id, out var entry);
                        bool CheckValidity(string platform) =>
                            platform != SocialMediaManager.SocialMediaUserEntry.NoLink;
                        List<string> linkedProfiles = new();
                        
                        if (CheckValidity(entry.Youtube)) linkedProfiles.Add("YouTube");
                        if (CheckValidity(entry.Twitter)) linkedProfiles.Add("Twitter");
                        if (CheckValidity(entry.DeviantArt)) linkedProfiles.Add("Deviantart");
                        if (CheckValidity(entry.Instagram)) linkedProfiles.Add("Instagram");
                        if (CheckValidity(entry.GitHub)) linkedProfiles.Add("GitHub");
                        if (CheckValidity(entry.Steam)) linkedProfiles.Add("Steam");
                        if (CheckValidity(entry.Reddit)) linkedProfiles.Add("Reddit");
                        if (CheckValidity(entry.ArtStation)) linkedProfiles.Add("ArtStation");
                        if (CheckValidity(entry.TikTok)) linkedProfiles.Add("TikTok");
                        if (CheckValidity(entry.Twitch)) linkedProfiles.Add("Twitch");
                        if (CheckValidity(entry.PersonalWebsite)) linkedProfiles.Add("Personal Website");

                        string[] values = selectMenu.Data.Values.Where(linkedProfiles.Contains).ToArray();
                        if (values.Length == 0)
                        {
                            selectMenu.RespondAsync("Cannot unlink profiles that have not been linked", ephemeral: true);
                            selectMenu.Message.DeleteAsync();
                            return;
                        }
                        
                        string okButtonID = $"button-{Helper.RandomInt(0, 1000)}";
                        string cancelButtonID = $"button-{Helper.RandomInt(0, 1000)}";
                        selectMenu.RespondAsync(components:FireMessageComponent.CreateNew(new FireMessageComponent(
                            new ComponentBuilder()
                                .WithButton("Unlink Selected Profiles", okButtonID,
                                    ButtonStyle.Danger)
                                .WithButton("Cancel", cancelButtonID, ButtonStyle.Secondary),
                            button =>
                            {
                                string buttonID = button.Data.CustomId;
                                if (buttonID == okButtonID)
                                {

                                    HashSet<string> unlinkees = new();

                                    string? DoReplace(string platform)
                                    {
                                        const string replacement = SocialMediaManager.SocialMediaUserEntry.NoLink;

                                        if (!values.Contains(platform)) return null;
                                        unlinkees.Add(platform);

                                        return replacement;
                                    }

                                    button.RespondAsync(ephemeral: true, text: SocialMediaManager.ModifyUser(
                                        cmd.User.Id,
                                        Youtube: DoReplace("YouTube"),
                                        Twitter: DoReplace("Twitter"),
                                        DeviantArt: DoReplace("Deviantart"),
                                        Instagram: DoReplace("Instagram"),
                                        GitHub: DoReplace("GitHub"),
                                        Steam: DoReplace("Steam"),
                                        Reddit: DoReplace("Reddit"),
                                        ArtStation: DoReplace("ArtStation"),
                                        TikTok: DoReplace("TikTok"),
                                        Twitch: DoReplace("Twitch"),
                                        PersonalWebsite: DoReplace("Personal Website"))
                                        ? $"Unlinked {string.Join(", ", unlinkees)}"
                                        : "The user does not exist in the database");
                                }
                                button.Message.DeleteAsync();
                                selectMenu.Message.DeleteAsync();
                            }, null)), text:"Are you sure you want to unlink the selected profiles?");
                    }).Create());
            }, devOnly), guild);

        #endregion
        }
    }
}