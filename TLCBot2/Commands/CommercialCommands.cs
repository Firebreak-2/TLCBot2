using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using TLCBot2.Cookies;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Commands
{
    public static class CommercialCommands
    {
        public static async Task Initialize()
        {
            var guild = Constants.Guilds.Lares!;
        
            #region Random Prompt Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                .WithName("prompt")
                .WithDescription("Generates a random prompt for you to draw!"), cmd =>
            {
                string prompt;
            
                string[] promptCharprops =    File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\charProps.cfg");
                string[] promptChars =        File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\chars.cfg");
                string[] promptColors =       File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\colors.cfg");
                string[] promptScenery =      File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\scenery.cfg");
                string[] promptSceneryprops = File.ReadAllLines($"{Program.FileAssetsPath}\\artToysPrompts\\sceneryProps.cfg");
        
                if (Helper.RandomInt(1, 20) == 1) // 1/20 chance to omit charProp
                    prompt =
                        $"{promptChars.RandomChoice()} {Regex.Replace(promptScenery.RandomChoice(), @"{.+?}", promptSceneryprops.RandomChoice())}";
                else
                    prompt =
                        $"{promptChars.RandomChoice()} {promptCharprops.RandomChoice().NamedFormat("color", promptColors.RandomChoice())} {promptScenery.RandomChoice().NamedFormat("sceneryprop",promptSceneryprops.RandomChoice())}";
        
                var embed = new EmbedBuilder()
                    .WithTitle($"Art prompt for {cmd.User.Username}")
                    .WithDescription(prompt)
                    .WithColor(Discord.Color.Blue);
                cmd.RespondAsync(embed: embed.Build());
            }), guild);
            #endregion
        
            #region Clear Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("clear")
                    .WithDescription("Deletes the number of messages specified")
                    .AddOption("amount", ApplicationCommandOptionType.Integer, "The specified amount"), 
                cmd =>
                {
                    long count = (long) cmd.Data.Options.First().Value;
                    switch (count)
                    {
                        case <= 0:
                            cmd.RespondAsync("You cant delete something that doesn't exist??", ephemeral:true);
                            return;
                        case > 50:
                            cmd.RespondAsync("dude.. that's wayyy too many channels. ", ephemeral:true);
                            return;
                    }
        
                    var messages = cmd.Channel.GetMessagesAsync((int)count, CacheMode.AllowDownload, RequestOptions.Default)
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
                }), guild);
            #endregion
        
            #region Ping Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("ping")
                    .WithDescription("Responds with \"pong!\" to indicate that the bot is online."), 
                cmd => cmd.RespondAsync("pong!")), guild);
            #endregion
        
            #region Cookies Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("cookies")
                    .WithDescription("Check the amount of 🍪 that you own!")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to check the cookies of."),
                cmd =>
                {
                    var user = (SocketUser?)(cmd.Data.Options.Any() ? cmd.Data.Options.First().Value : null);

                    if (!CookieManager.GetUserFromDatabase(user?.Id ?? cmd.User.Id, out var cookies, out _))
                        cookies = 0;

                    var embed = new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
                        .WithTitle($"{user?.Username ?? cmd.User.Username}'s balance: {cookies}");
                
                    cmd.RespondAsync(embed:embed.Build());
                }), guild);
            #endregion
        
            #region Cookie Leaderboard Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("leaderboard")
                    .WithDescription("Shows the current 🍪 leaderboard."),
                cmd =>
                {
                    int i = 0;
                    string output = "";
                    var lines = File.ReadAllLines(CookieManager.CookieDatabasePath)
                        .OrderByDescending(x => CookieManager.DeformatUserData(x).cookies);

                    if (!lines.Any())
                    {
                        cmd.RespondAsync("Leaderboard count is currently 0, cannot function.", ephemeral:true);
                        return;
                    }
                
                    var g = cmd.Channel.GetGuild();

                    foreach (var line in lines)
                    {
                        var (userId, cookies, isBanned) = CookieManager.DeformatUserData(line);

                        if (userId == 0) continue;

                        string rankingPrefix = i switch
                        {
                            0 => "🥇",
                            1 => "🥈",
                            2 => "🥉",
                            _ => $"#{i+1}"
                        };

                        string banDash = isBanned ? "~~" : "";
                        string userMention = $"<@!{userId}>";

                        output += $"{rankingPrefix}: **{cookies}**🍪  {banDash}{userMention}{banDash}\n";
                    
                        if (++i >= 10) break;
                    }
                    output = output[..^1];

                    var embed = new EmbedBuilder()
                        .WithTitle("TLC 🍪 Leaderboard")
                        .WithColor(Discord.Color.Blue)
                        .WithDescription(output)
                        .WithCurrentTimestamp();
                
                    cmd.RespondAsync(embed:embed.Build());
                }), guild);
            #endregion
        
            #region Give Cookie Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("give-cookie")
                    .WithDescription("Adds or removes(using negatives) 🍪 from a user.")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to manipulate the 🍪 of",true)
                    .AddOption("count", ApplicationCommandOptionType.Integer, "The amount of 🍪 to give to the person."),
                cmd =>
                {
                    var user = (SocketUser)cmd.Data.Options.First().Value;
                    var count = cmd.Data.Options.Count == 2 ? Convert.ToInt32((long)cmd.Data.Options.Last().Value) : 5;

                    CookieManager.TakeOrGiveCookiesToUser(user.Id, count);
                    CookieManager.GetUserFromDatabase(user.Id, out var cookies, out _);

                    bool isPositive = count >= 0;
            
                    var embed = new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
                        .WithTitle($"{(isPositive ? "Given" : "Taken")} `{Math.Abs(count)}` 🍪 {(isPositive ? "to" : "from")} {user.Username}.")
                        .WithDescription($"current balance: {cookies}");
            
                    cmd.RespondAsync(embed:embed.Build());
                }, true), guild);
            #endregion
        
            #region Set Cookie Command
            await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
                    .WithName("set-cookies")
                    .WithDescription("Sets the amount of 🍪 to a specific number for a specific user.")
                    .AddOption("user", ApplicationCommandOptionType.User, "The user to manipulate the 🍪 of",true)
                    .AddOption("amount", ApplicationCommandOptionType.Integer, "The set number of 🍪."),
                cmd =>
                {
                    var user = (SocketUser)cmd.Data.Options.First().Value;
                    var count = cmd.Data.Options.Count == 2 ? Convert.ToInt32((long)cmd.Data.Options.Last().Value) : 0;

                    CookieManager.GetUserFromDatabase(user.Id, out var oldCookies, out _);
                    CookieManager.AddOrEditUserToDatabase(user.Id, count);

                    var embed = new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
                        .WithTitle($"Changed the 🍪 of {user.Username}.")
                        .WithDescription($"{oldCookies} → {count}");
            
                    cmd.RespondAsync(embed:embed.Build());
                }, true), guild);
            #endregion
        }
    }
}