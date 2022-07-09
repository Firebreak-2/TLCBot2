using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("prompt", "Generates a random prompt for you to draw")]
    public async Task Prompt()
    {
        await RespondAsync(embed: await GenerateRandomPromptEmbed(),
            components: new ComponentBuilder()
                .WithButton("Reroll", "prompt-reroll-button")
                .Build());
    }

    [ComponentInteraction("prompt-reroll-button")]
    public async Task PromptReRollButtonResponse()
    {
        var embed = await GenerateRandomPromptEmbed();
        await ((SocketMessageComponent) Context.Interaction).Message.ModifyAsync(props =>
        {
            props.Embed = embed;
        });
        await DeferAsync();
    }
    
    private static Task<Embed> GenerateRandomPromptEmbed()
    {
        string[] promptCharProps =    File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/charProps.cfg");
        string[] promptChars =        File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/chars.cfg");
        string[] promptColors =       File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/colors.cfg");
        string[] promptScenery =      File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/scenery.cfg");
        string[] promptSceneryProps = File.ReadAllLines($"{Program.FileAssetsPath}/artToysPrompts/sceneryProps.cfg");

        string character = promptChars.RandomChoice();
        string scenery = promptScenery.RandomChoice().NamedFormat("sceneryprop", promptSceneryProps.RandomChoice());
        string characterProp = promptCharProps.RandomChoice().NamedFormat("color", promptColors.RandomChoice());
        
        string prompt = Helper.Rando.Bool()
            ? $"{character} {characterProp} {scenery}"
            : $"{character} {scenery}";

        return Task.FromResult(new EmbedBuilder()
            .WithTitle("Random Art Prompt")
            .WithDescription($"{prompt[..1].ToUpper()}{prompt[1..]}.")
            .WithColor(Color.Blue)
            .Build());
    }
}