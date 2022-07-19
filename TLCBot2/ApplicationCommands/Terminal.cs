using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.CommandLine.Commands;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    // [SlashCommand("terminal", "Executes a terminal command and responds with the output if any")]
    public async Task Terminal([Autocomplete(typeof(TerminalAutocompleteHandler))] string command)
    {
        await DeferAsync();
    }

    public class TerminalAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            var result = await InternalHandler(context, autocompleteInteraction, parameter);

            if (!result.Any())
                return AutocompletionResult.FromSuccess();
            
            return AutocompletionResult.FromSuccess(result.Select(x => 
                    new AutocompleteResult(x.Preview, x.Value)).Take(25));
        }

        private static async Task<IEnumerable<(string Preview, string Value)>> InternalHandler(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter)
        {
            string input = autocompleteInteraction.Data.Current.Value.ToString() ?? "";

            var allCommands = TerminalCommands.All
                .Select(x => (x.Method.Name, 
                    $"{x.Method.Name} {string.Join(" ", x.Method.GetParameters().Select(p => $"<{p.ParameterType.Name} {p.Name}>"))}"));

            string[] parts = input.Split(' ');

            return parts.Length switch
            {
                0 => allCommands,
                1 => allCommands.Where(x => x.Name.StartsWith(parts[0])),
                _ => Array.Empty<(string, string)>()
            };
        }
    }
}