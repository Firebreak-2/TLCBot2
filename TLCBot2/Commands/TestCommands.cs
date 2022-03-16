using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using MoreLinq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Utilities;
using SkiaSharp;
using TLCBot2.Core;
using TLCBot2.DataManagement;
using Color = Discord.Color;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.Commands;

public static class TestCommands
{
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;

        #region Spawn Button Command
        await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            .WithName("spawn-button")
            .WithDescription("spawns a button"), cmd =>
        {
            cmd.RespondWithModalAsync(new ModalBuilder()
                .WithTitle("title")
                .WithCustomId("modal-1")
                .AddTextInput(new TextInputBuilder()
                    .WithLabel("input label")
                    .WithCustomId($"test-input-{Helper.RandomInt(0, 1000)}")
                    .WithStyle(TextInputStyle.Paragraph)
                    .WithRequired(true)
                ).AddTextInput(new TextInputBuilder()
                    .WithLabel("input label 2")
                    .WithCustomId($"test-input-{Helper.RandomInt(0, 1000)}")
                    .WithRequired(true)
                ).AddTextInput(new TextInputBuilder()
                    .WithLabel("input label 3")
                    .WithCustomId($"test-input-{Helper.RandomInt(0, 1000)}")
                    .WithRequired(true)
                ).AddTextInput(new TextInputBuilder()
                    .WithLabel("input label 4")
                    .WithCustomId($"test-input-{Helper.RandomInt(0, 1000)}")
                    .WithRequired(true)
                ).AddTextInput(new TextInputBuilder()
                    .WithLabel("input label 5")
                    .WithCustomId($"test-input-{Helper.RandomInt(0, 1000)}")
                    .WithRequired(true)
                ).Build()
            );
        }, true), guild);
        #endregion

        #region Spawn Button Command
        await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            .WithName("poll")
            .WithDescription("Conducts a poll"), cmd =>
        {
            EmbedBuilder embed = new();
            
            var cb = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                .WithSelectMenu($"poll-{Helper.RandomInt(0, 1000)}", new List<SelectMenuOptionBuilder>
                {
                    new SelectMenuOptionBuilder()
                        .WithLabel("one")
                        .WithValue("one"),
                    new SelectMenuOptionBuilder()
                        .WithLabel("two")
                        .WithValue("two")
                }, maxValues: 1), null, selectMenu =>
            {
                var selectedEmbed = selectMenu.Message.Embeds.First();
                selectMenu.Message.ModifyAsync(props =>
                    props.Embed = selectedEmbed.ToEmbedBuilder()
                        .WithFields(selectedEmbed.Fields.Select(x => 
                            new EmbedFieldBuilder()
                                .WithName(x.Name)
                                .WithValue(x.Name == selectMenu.Data.Value 
                                    ? $"{int.Parse(x.Value) + 1}" 
                                    : x.Value)))
                        .Build());
                
                selectMenu.RespondAsync("Response submitted.", ephemeral:true);
            }));

            embed.WithTitle("This is a poll").WithColor(Color.Blue);
            foreach (var option in ((SelectMenuComponent)cb.Components.First().Components.First()).Options)
            {
                embed.AddField(option.Label, 0);
            }
            
            
            cmd.RespondAsync(embed: embed.Build(), components:cb);
        }, true), guild);
        #endregion
        
        #region Social Media Command
        await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            .WithName("social-media")
            .WithDescription("Displays a person's linked social media accounts")
            .AddOption("user", ApplicationCommandOptionType.User, "The user to check the socials of", true),
            cmd =>
            {
                SocketUser user = (SocketUser) cmd.Data.Options.First().Value;
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
            }), guild);
        #endregion
        
        #region Link Social Media Profile Command
        await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            .WithName("link-clover-variation")
            .WithDescription("Links your discord account with a social media profile")
            .AddOption("profile-link", ApplicationCommandOptionType.String, "The **link** to your social media profile", true),
            cmd =>
            {
                string input = (string) cmd.Data.Options.First().Value;

                var matcher = new Regex(@"https?:\/\/(?:www\.)?(.+)\.(?:tv|com)\/?(?:.+)?");
                var match = matcher.Match(input);
                if (!match.Success)
                {
                    cmd.RespondAsync("Input is not a link", ephemeral: true);
                    return;
                }

                ulong userId = cmd.User.Id;
                switch (matcher.Replace(input, "$1").ToLower())
                {
                    case "youtube":
                        SocialMediaManager.AddOrModifyUser(userId, Youtube: match.Value);
                        cmd.RespondAsync($"Linked your YouTube profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "twitter":
                        SocialMediaManager.AddOrModifyUser(userId, Twitter: match.Value);
                        cmd.RespondAsync($"Linked your Twitter profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "deviantart":
                        SocialMediaManager.AddOrModifyUser(userId, DeviantArt: match.Value);
                        cmd.RespondAsync($"Linked your DeviantArt profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "instagram":
                        SocialMediaManager.AddOrModifyUser(userId, Instagram: match.Value);
                        cmd.RespondAsync($"Linked your Instagram profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "github":
                        SocialMediaManager.AddOrModifyUser(userId, GitHub: match.Value);
                        cmd.RespondAsync($"Linked your GitHub profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "steamcommunity":
                        SocialMediaManager.AddOrModifyUser(userId, Steam: match.Value);
                        cmd.RespondAsync($"Linked your Steam profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "reddit":
                        SocialMediaManager.AddOrModifyUser(userId, Reddit: match.Value);
                        cmd.RespondAsync($"Linked your Reddit profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "artstation":
                        SocialMediaManager.AddOrModifyUser(userId, ArtStation: match.Value);
                        cmd.RespondAsync($"Linked your ArtStation profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "tiktok":
                        SocialMediaManager.AddOrModifyUser(userId, TikTok: match.Value);
                        cmd.RespondAsync($"Linked your TikTok profile with the link {match.Value}", ephemeral:true);
                        break;
                    case "twitch":
                        SocialMediaManager.AddOrModifyUser(userId, Twitch: match.Value);
                        cmd.RespondAsync($"Linked your Twitch profile with the link {match.Value}", ephemeral:true);
                        break;
                    default:
                        SocialMediaManager.AddOrModifyUser(userId, PersonalWebsite: match.Value);
                        cmd.RespondAsync($"Linked your Personal Website with the link {match.Value}", ephemeral:true);
                        break;
                }
            }), guild);
        #endregion
        
        #region Link Social Media Profile Command
        await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            .WithName("link-fire-variation")
            .WithDescription("Links your discord account with a social media profile")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("platform")
                .WithDescription("The social media platform to be linked to")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
                .AddChoice("YouTube", "youtube")
                .AddChoice("Twitter", "twitter")
                .AddChoice("DeviantArt", "deviantart")
                .AddChoice("Instagram", "instagram")
                .AddChoice("GitHub", "github")
                .AddChoice("Steam", "steam")
                .AddChoice("Reddit", "reddit")
                .AddChoice("ArtStation", "artstation")
                .AddChoice("TikTok", "tiktok")
                .AddChoice("Twitch", "twitch")
                .AddChoice("Personal Website", "website")
            )
            .AddOption("profile-link", ApplicationCommandOptionType.String, "The **link** to your social media profile", true),
            cmd =>
            {
                string platform = (string) cmd.Data.Options.First().Value;
                string profLink = (string) cmd.Data.Options.Last().Value;

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
                    case "steam":
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
            }), guild);
        #endregion
    }
}