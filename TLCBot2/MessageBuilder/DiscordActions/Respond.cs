using Discord;
using Discord.WebSocket;
using TLCBot2.Attributes;
using TLCBot2.Types;

namespace TLCBot2.MessageBuilder.DiscordActions;

public static partial class DiscordMethods
{
    [DiscordAction(NoDefer = true)]
    public static void Respond(SocketInteraction interaction, MessageData message, bool ephemeral = false)
    {
        Task.Run(async () => await interaction.RespondAsync(
            message.Text, 
            embed: message.Embed?.ToEmbed(), 
            ephemeral: ephemeral,
            components: MessageData.MessageComponentsData.RowsToMessageComponent(message.ComponentRows)));
    }
}