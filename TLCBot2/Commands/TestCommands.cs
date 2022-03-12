using Discord;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Commands;

public static class TestCommands
{
    public static async Task Initialize()
    {
        var guild = Constants.Guilds.Lares;

        #region Spawn Button Command
        await FireCommand.CreateNew(new FireCommand(new SlashCommandBuilder()
            .WithName("spawn-button")
            .WithDescription("spawns a button"), cmd =>
        {
            var cb = FireMessageComponent.CreateNew(new FireMessageComponent(new ComponentBuilder()
                .WithSelectMenu("selectmenu-1", new List<SelectMenuOptionBuilder>
                {
                    new SelectMenuOptionBuilder()
                        .WithLabel("one")
                        .WithValue("invalid")
                        .WithDescription("desc-1"),
                    new SelectMenuOptionBuilder()
                        .WithLabel("two")
                        .WithValue("valid")
                        .WithDescription("desc-2")
                }, maxValues: 1), null, selectMenu =>
            {
                var text = string.Join(", ", selectMenu.Data.Values);
        
                Constants.Channels.Lares.DefaultFileDump
                    .SendMessageAsync($"`{selectMenu.User.Username}` selected `{text}`.\nSelected on: <{selectMenu.Message.GetJumpUrl()}>");
                
                selectMenu.RespondAsync("Response submitted.", ephemeral:true);
            }));
            
            cmd.RespondAsync(components:cb);
        }, true), guild);
        #endregion
    }
}