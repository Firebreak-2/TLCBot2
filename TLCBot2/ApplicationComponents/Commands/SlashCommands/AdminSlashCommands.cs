using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.DataManagement;
using TLCBot2.DataManagement.Temporary;
using TLCBot2.Utilities;
using Color = Discord.Color;

namespace TLCBot2.ApplicationComponents.Commands.SlashCommands;

public static class AdminSlashCommands
{
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;
        const bool devOnly = true;
        {
            
            #region Spawn Button Command

            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                .WithName("spawn-button")
                .WithDescription("spawns a button"), cmd =>
            {
                
            }, devOnly), guild);
            #endregion

            #region Give Cookie Command

            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                    .WithName("give-cookie")
                    .WithDescription("Adds or removes(using negatives) 🍪 from a user.")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to manipulate the 🍪 of", true)
                    .AddOption("count", ApplicationCommandOptionType.Integer, "The amount of 🍪 to give to the person.")
                    .AddOption("reason", ApplicationCommandOptionType.String, "The reason for giving the 🍪s."),
                cmd =>
                {
                    SocketUser user = (SocketUser) cmd.Data.Options.First(x => x.Name == "user").Value;
                    int count = cmd.Data.Options.Count == 2
                        ? Convert.ToInt32((long) cmd.Data.Options.First(x => x.Name == "count").Value)
                        : 5;
                    string? reason = (string) cmd.Data.Options.FirstOrDefault(x => x.Name == "reason")!.Value;

                    CookieManager.TakeOrGiveCookiesToUser(user.Id, count, reason);
                    CookieManager.GetUser(user.Id, out var entry);
                    bool isPositive = count >= 0;

                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle(
                            $"{(isPositive ? "Given" : "Taken")} `{Math.Abs(count)}` 🍪 {(isPositive ? "to" : "from")} {user.Username}.")
                        .WithDescription($"current balance: {entry.Cookies}");

                    cmd.RespondAsync(embed: embed.Build());
                }, devOnly), guild);

            #endregion

            #region Set Cookie Command

            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                    .WithName("set-cookies")
                    .WithDescription("Sets the amount of 🍪 to a specific number for a specific user.")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to manipulate the 🍪 of", true)
                    .AddOption("amount", ApplicationCommandOptionType.Integer, "The set number of 🍪.", true)
                    .AddOption("reason", ApplicationCommandOptionType.String, "The reason for giving the 🍪s."),
                cmd =>
                {
                    SocketUser user = (SocketUser) cmd.Data.Options.First(x => x.Name == "user").Value;
                    int count = cmd.Data.Options.Count == 2
                        ? Convert.ToInt32((long) cmd.Data.Options.First(x => x.Name == "amount").Value)
                        : 5;
                    string? reason = (string) cmd.Data.Options.FirstOrDefault(x => x.Name == "reason")!.Value;

                    CookieManager.GetUser(user.Id, out var entry);
                    CookieManager.AddOrModifyUser(user.Id, count, reason: reason);

                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithTitle($"Changed the 🍪 of {user.Username}.")
                        .WithDescription($"{entry?.Cookies ?? 0} → {count}");

                    cmd.RespondAsync(embed: embed.Build(), ephemeral: true);
                }, devOnly), guild);

            #endregion

            #region Clear Command

            await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                    .WithName("clear")
                    .WithDescription("Deletes the number of messages specified")
                    .AddOption("amount", ApplicationCommandOptionType.Integer, "The specified amount", true),
                cmd =>
                {
                    long count = (long) cmd.Data.Options.First().Value;
                    switch (count)
                    {
                        case <= 0:
                            cmd.RespondAsync("You cant delete something that doesn't exist??", ephemeral: true);
                            return;
                        case > 50:
                            cmd.RespondAsync("dude.. that's wayyy too many channels. ", ephemeral: true);
                            return;
                    }

                    var messages = cmd.Channel
                        .GetMessagesAsync((int) count, CacheMode.AllowDownload, RequestOptions.Default)
                        .ToArrayAsync().Result.First().ToArray();

                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            cmd.Channel.DeleteMessageAsync(messages[i], RequestOptions.Default);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        cmd.RespondAsync("An error has occured. If this problem prevails, contact `Firebreak#3813`.",
                            ephemeral: true);
                        return;
                    }

                    cmd.RespondAsync($"`{count}` messages deleted.", ephemeral: true);
                }, devOnly), guild);

            #endregion
        }
    }
}