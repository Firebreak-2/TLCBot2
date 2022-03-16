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
            
        }, true), guild);
        #endregion

        #region Poll Command
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
    }
}