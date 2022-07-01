using Discord.Interactions;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("echo", "Repeats the given text in the channel the command is executed in")]
    public async Task Echo(string textToRepeat)
    {
        await Context.Channel.SendMessageAsync(textToRepeat);
        await RespondAsync("Message Sent!", ephemeral: true);
    }
}