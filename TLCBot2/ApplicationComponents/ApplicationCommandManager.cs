using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Commands.MessageCommands;
using TLCBot2.ApplicationComponents.Commands.SlashCommands;
using TLCBot2.ApplicationComponents.Commands.UserCommands;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents;

public static class ApplicationCommandManager
{
    public static List<FireSlashCommand> AllSlashCommands = new();
    public static List<FireUserCommand> AllUserCommands = new();
    public static List<FireMessageCommand> AllMessageCommands = new();
    public static Dictionary<string,(string name,string applicationCommandType,string description,string functionCall,(string name,string description)[] parameters)>HelpCommandResponses = new();
    public static IEnumerable<ApplicationCommandProperties> AllCommands => AllSlashCommands
        .Select(x => x.Slashie)
        .Union<ApplicationCommandProperties>(AllUserCommands
            .Select(x => x.Command))
        .Union(AllMessageCommands
            .Select(x => x.Command));
    public static async Task Initialize()
    {
        await AdminSlashCommands.Initialize();
        await AdminUserCommands.Initialize();
        await AdminMessageCommands.Initialize();
        await CommercialUserCommands.Initialize();
        await CommercialMessageCommands.Initialize();
        await CommercialSlashCommands.Initialize();

        await InitializeHelpCommand();

        string PascalCase(string input) =>
            Regex.Replace(input,
                    @"(?<=-).|^.", data => data.Value.ToUpper())
                .Replace("-", "");
        foreach (var props in AllSlashCommands)
        {
            var parameters = props.Builder.Options ?? new();
            string name = props.Builder.Name;
            string functionCall = $"{PascalCase(name)}";
            functionCall += $"({string.Join(", ", parameters.Select(x => $"`{Enum.GetName(x.Type)}` {x.Name}"))})\n";
            
            HelpCommandResponses.Add(name, 
                (
                    name,
                    "Slash Command",
                    props.Builder.Description,
                    functionCall,
                    parameters.Select(x => (x.Name, x.Description)).ToArray())
                );
        }
        
        await Constants.Guilds.TlcBetaTesting!.BulkOverwriteApplicationCommandAsync(AllCommands.ToArray());
        
        // if (allCommands.Any(x => x.Guild == null))
        //     await Program.Client.BulkOverwriteGlobalApplicationCommandsAsync(allCommands
        //         .Where(x => x.Guild == null).ToArray());
    }

    private static async Task InitializeHelpCommand()
    {
        var optionBuilder = new SlashCommandOptionBuilder()
            .WithName("command-name")
            .WithType(ApplicationCommandOptionType.String)
            .WithDescription("the command to give help info on")
            .WithRequired(true)
            // .WithAutocomplete(true)
            ;

        // optionBuilder.Choices = new List<ApplicationCommandOptionChoiceProperties>
        // {
        //     new()
        //     {
        //         Name = "alpha",
        //         Value = "alpha"
        //     }
        // };

        // foreach (var command in AllSlashCommands)
        // {
        // }
        
        await FireSlashCommand.CreateNew(new FireSlashCommand(new SlashCommandBuilder()
                .WithName("help")
                .WithDescription("Gives you information about the command you input")
                .AddOption(optionBuilder),
            cmd =>
            {
                string commandRequest = (string) cmd.Data.Options.First().Value;
                if (HelpCommandResponses.TryGetValue(commandRequest, out var response))
                {
                    var embed = new EmbedBuilder()
                        .WithTitle(response.name)
                        .WithDescription(response.description)
                        .WithColor(Color.Blue)
                        .AddField("Function", response.functionCall);

                    foreach ((string name, string description) in response.parameters)
                    {
                        embed.AddField(name, description);
                    }

                    cmd.RespondAsync(embed: embed.Build());
                    return;
                }

                cmd.RespondAsync($"No command with the name {commandRequest} found", ephemeral: true);
            }), Constants.Guilds.TlcBetaTesting);
    }
}