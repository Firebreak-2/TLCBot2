using Discord.Interactions;
using TLCBot2.Core;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("ping", "Responds with the bot's latency to Discord's servers")]
    public async Task Ping()
    {
        await RespondAsync($"The latency is {Program.Client.Latency}", ephemeral: true);
    }
}