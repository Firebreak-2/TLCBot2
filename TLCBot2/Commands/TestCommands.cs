using System.Net;
using System.Text.RegularExpressions;
using Discord;
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
                .WithSelectMenu($"poll-answers-{Helper.RandomInt(0, 1000)}", new List<SelectMenuOptionBuilder>
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
        
        #region Color Photo Command
        // await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
        //     .WithName("color-photo")
        //     .WithDescription("Picks out the most prominent colors in a photo")
        //     .AddOption("image", ApplicationCommandOptionType.Attachment, "The image to check the colors of", true),
        //     cmd =>
        //     {
        //         const int colorsToPick = 3;
        //         const int tileWidth = 200;
        //         const int tileHeight = 100;
        //         using var image = new Image<Argb32>(tileWidth, tileHeight * colorsToPick);
        //         using var imgInput = Image.Load<Argb32>(((Discord.Attachment) cmd.Data.Options.First().Value).);
        //         
        //         var uniqueColors = new Dictionary<Argb32, int>();
        //         imgInput.FillColor((_, _, pixel) =>
        //         {
        //             if (!uniqueColors.TryGetValue(pixel, out int prevCount))
        //                 uniqueColors.Add(pixel, 1);
        //             else
        //             {
        //                 uniqueColors.Remove(pixel);
        //                 uniqueColors.Add(pixel, prevCount + 1);
        //             }
        //
        //             return SixLabors.ImageSharp.Color.White;
        //             
        //         });
        //         uniqueColors = uniqueColors.OrderByDescending(x => x.Value).ToDictionary();
        //         Argb32[] colorsToDisplay;
        //         colorsToDisplay = uniqueColors.Count > colorsToPick
        //             ? uniqueColors.Where((_, i) => i <= colorsToPick)
        //                 .Select(x => x.Key).ToArray()
        //             : uniqueColors.Select(x => x.Key).ToArray();
        //         
        //         image.FillColor((_, y) => colorsToDisplay[y / (tileHeight * colorsToPick / colorsToDisplay.Length)]);
        //         
        //         var embed = new EmbedBuilder()
        //             .WithTitle("Most Prominent Colors")
        //             .WithDescription("gamer\ngamer\ngamer")
        //             .WithColor(Color.Blue)
        //             .WithImageUrl(Helper.GetFileUrl(image.ToStream()));
        //     
        //     cmd.RespondAsync(embed:embed.Build());
        // }), guild);
        #endregion
    }
}