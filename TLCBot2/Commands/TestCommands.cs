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
            var cb = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                .WithSelectMenu("selectmenu-1", new List<SelectMenuOptionBuilder>
                {
                    new SelectMenuOptionBuilder()
                        .WithLabel("one")
                        .WithValue("invalid")
                        .WithDescription("desc-1"),
                    new SelectMenuOptionBuilder()
                        .WithLabel("two")
                        .WithValue("valid")
                        .WithDescription("desc-2")
                }, maxValues: 1), null, selectMenu =>
            {
                string text = string.Join(", ", selectMenu.Data.Values);
        
                Constants.Channels.Lares.DefaultFileDump
                    .SendMessageAsync($"`{selectMenu.User.Username}` selected `{text}`.\nSelected on: <{selectMenu.Message.GetJumpUrl()}>");
                
                selectMenu.RespondAsync("Response submitted.", ephemeral:true);
            }));
            
            cmd.RespondAsync(components:cb);
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

                const string noLink = SocialMediaManager.SocialMediaUserEntry.NoLink;

                if (entry.Twitter         != noLink) embed.AddField("Twitter"         , entry.Twitter        );
                if (entry.Youtube         != noLink) embed.AddField("YouTube"         , entry.Youtube        );
                if (entry.Twitch          != noLink) embed.AddField("Twitch"          , entry.Twitch         );
                if (entry.TikTok          != noLink) embed.AddField("TikTok"          , entry.TikTok         );
                if (entry.Instagram       != noLink) embed.AddField("Instagram"       , entry.Instagram      );
                if (entry.DeviantArt      != noLink) embed.AddField("DeviantArt"      , entry.DeviantArt     );
                if (entry.ArtStation      != noLink) embed.AddField("ArtStation"      , entry.ArtStation     );
                if (entry.Reddit          != noLink) embed.AddField("Reddit"          , entry.Reddit         );
                if (entry.Steam           != noLink) embed.AddField("Steam"           , entry.Steam          );
                if (entry.GitHub          != noLink) embed.AddField("GitHub"          , entry.GitHub         );
                if (entry.PersonalWebsite != noLink) embed.AddField("Personal Website", entry.PersonalWebsite);

                if (!embed.Fields.Any())
                {
                    cmd.RespondAsync($"{user.Username} has not linked any of their social media profiles.");
                    return;
                }

                cmd.RespondAsync(embed: embed.Build());
            }), guild);
        #endregion
    }
}