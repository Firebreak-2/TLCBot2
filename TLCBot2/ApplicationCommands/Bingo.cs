using Discord;
using Discord.Interactions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TLCBot2.Core;
using TLCBot2.Utilities;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("bingo", "Generates a bingo board for you to use for drawing ideas")]
    public async Task Bingo([MinValue(3)] [MaxValue(11)] int tileCount = 5)
    {
        if (tileCount % 2 == 0)
        {
            await RespondAsync("Tile count must be an **odd** number.", ephemeral: true);
            return;
        }

        await RespondAsync("Bingo card creation queued...");
        ulong messageId = (await Context.Interaction.GetOriginalResponseAsync()).Id;

        Embed embed = await Task.Run(async () => await GetBingoEmbed(tileCount));

        await Context.Interaction.ModifyOriginalResponseAsync(props =>
        {
            props.Content = "";
            props.Embed = embed;
            props.Components = new ComponentBuilder()
                .WithButton("Reroll", $"reroll-button;{messageId},{tileCount}")
                .Build();
        });
    }

    [ComponentInteraction("reroll-button;*,*")]
    public async Task ReRollButtonExecuted(string id, string tileCount)
    {
        Embed embed = await Task.Run(async () => await GetBingoEmbed(int.Parse(tileCount)));

        await Context.Channel.ModifyMessageAsync(ulong.Parse(id), props =>
        {
            props.Embed = embed;
        });
        
        await RespondAsync();
    }

    private async Task<Embed> GetBingoEmbed(int tilesPerRow)
    {
        using var image = new Image<Argb32>(1000, 1000);
        var bingoPrompts =
            (await Task.Run(() => File.ReadAllLines($"{Program.FileAssetsPath}/bingoPrompts.cfg")))
                .Select(x => x.Replace(' ', '\n')).ToHashSet();

        const int gridLineWidth = 5;
        int tileWidth = image.Width / tilesPerRow;
        int tileHeight = image.Height / tilesPerRow;
        int width = image.Width;

        image.FillColor((x, y) =>
        {
            for (int i = 0; i < gridLineWidth; i++)
            {
                for (int j = 0; j < tilesPerRow; j++)
                {
                    int val = i + width / tilesPerRow * j;
                    if (val <= gridLineWidth) continue;

                    if (x == val || y == val) return SixLabors.ImageSharp.Color.Black;
                }
            }

            return SixLabors.ImageSharp.Color.White;
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
                Font font = Helper.ArialFont.CreateFont(prompt.Length > 8 ? 35 : 42);
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

        using (var imgToBeDrawn = Image.Load<Argb32>($"{Program.FileAssetsPath}/TLC_Logo.png"))
        {
            imgToBeDrawn.Mutate(img =>
                img.Resize(tileWidth, tileHeight));
            image.Mutate(img =>
                img.DrawImage(imgToBeDrawn, centerPos, 1));
        }
        
        await using var stream = image.ToStream();
        string url = await Helper.GetFileUrl(stream);
        return new EmbedBuilder()
            .WithTitle($"TLC bingo card for {Context.User.Username}")
            .WithDescription(
                "Draw an image that would score a bingo on the following sheet." +
                " Don't forget to shout bingo and share your finished drawing!")
            .WithImageUrl(url)
            .WithAuthor(Context.User)
            .WithColor(Discord.Color.Blue)
            .Build();
    }
}