using TLCBot2.Attributes;
using TLCBot2.CommandLine.Commands;

namespace TLCBot2.Data;

public static class BackendServer
{
    // [Initialize]
    public static async Task Initialize()
    {
        if (RuntimeConfig.RuntimeConfig.BackendServer is not { } guild)
            return;
        
        var list = TerminalCommands.GetCategoricalRoles()
            .SelectMany(role => role.Value)
            .Select(x => x.Role)
            .Concat(RuntimeConfig.RuntimeConfig.ModProfessionRoles.Select(x => x.GetRole()))
            .Where(x => x.Color.RawValue > 0)
            .ToList();
        
        foreach (var role in list)
        {
            await TerminalCommands.GenerateEmoji(role.Color.ToString(), false, true);
        }

        foreach (var emote in guild.Emotes)
        {
            if (list.All(x => $"e_{x.Color.ToString()[1..]}" != emote.Name))
                await guild.DeleteEmoteAsync(emote);
        }
    }
}