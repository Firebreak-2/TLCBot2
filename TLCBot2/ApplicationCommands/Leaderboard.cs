using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Humanizer;
using TLCBot2.Data;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("leaderboard", "Shows a leaderboard of the top 10 cookie user13s by amount")]
    public async Task Leaderboard()
    {
        StringBuilder stringBuilder = new();
        await using var db = new TlcDbContext();
        var top10 = db.Users
            .OrderByDescending(x => x.Balance)
            .ToArray()
            .Where((_, i) => i < 10)
            .ToArray();
        for (int i = 0; i < top10.Length; i++)
        {
            string place = i > 2 
                ? $"#{i + 1}"
                : $":{(i + 1).ToOrdinalWords()}_place:";
            SocketGuildUser user = RuntimeConfig.FocusServer!.GetUser(top10[i].UserId);
            stringBuilder.Append($"{place}: {top10[i].Balance} :cookie: {user.Mention}\n");
        }

        var embedBuilder = new EmbedBuilder()
            .WithTitle("TLC :cookie: Leaderboard")
            // .WithFooter("Your rank is #NN • You have N 🍪")
            .WithDescription(stringBuilder.ToString());

        await RespondAsync(embed: embedBuilder.Build());
    }
}