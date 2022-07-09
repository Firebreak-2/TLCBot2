using Discord.Rest;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TLCBot2.Attributes;
using TLCBot2.CommandLine.Commands;
using TLCBot2.Core;
using TLCBot2.Utilities;
using Image = Discord.Image;

namespace TLCBot2.Data;

public static class BackendServer
{
    // [Initialize]
    public static async Task Initialize()
    {
        if (RuntimeConfig.RuntimeConfig.BackendServer is not { } guild)
            return;
        
        var roles = RuntimeConfig.RuntimeConfig.FocusServer!.Roles.ToList();
        var currentEmotes = guild.Emotes.ToList();
            
        foreach (var emote in currentEmotes)
        {
            roles.RemoveAll(x => x.Color.RawValue == emote.Name.To<uint>()
                                 || !TerminalCommands.RoleTagRegex.IsMatch(x.Name));

            await guild.DeleteEmoteAsync(emote);
        }

        foreach (var role in roles)
        {
            if (guild.Emotes.Count >= 50)
                throw new Exception("Backend server's emote capacity exceeded");
            
            using var image = new Image<Argb32>(100, 100);
            image.FillColor(role.Color.DiscordColorToArgb32());
            
            await guild.CreateEmoteAsync($"{role.Color.RawValue}".PadLeft(6, '0'), new Image(image.ToStream()));
        }
    }
}