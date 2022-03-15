using Discord;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.Listeners;

public static class StarboardListener
{
    public const int MinReactionsToPost = 6;
    public static string PostHistoryPath => $"{Program.FileAssetsPath}\\starboardPostHistory.txt";

    public static void Initialize()
    {
        if (!File.Exists(PostHistoryPath))
            File.WriteAllText(PostHistoryPath, "");
    }
    public static Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        var msg = message.GetOrDownloadAsync().Result;
        if (RuntimeConfig.WhitelistedStarboardChannels.Select(x => x.Id).Contains(msg.Channel.Id)
            && msg.Reactions.Count(x => x.Key.Name == "⭐") >= MinReactionsToPost
            && !File.ReadAllText(PostHistoryPath).Contains($"{msg.Id}"))
        {
            var embed = new EmbedBuilder()
                .WithAuthor(msg.Author)
                .WithColor(Color.Blue)
                .WithDescription(msg.Content);

            if (msg.Embeds.Any())
                embed.WithImageUrl(msg.Embeds.First().Image!.Value.Url);
            else if (msg.Attachments.Any())
                embed.WithImageUrl(msg.Attachments.First().Url);

            ((SocketTextChannel) RuntimeConfig.StarboardChannel).SendMessageAsync(embed:embed.Build());
            File.AppendAllText(PostHistoryPath, $"{msg.Id}");
        }

        return Task.CompletedTask;
    }
}