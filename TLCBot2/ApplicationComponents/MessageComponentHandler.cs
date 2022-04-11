using System.ComponentModel;
using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.ApplicationComponents.Eternal;
using TLCBot2.Core;
using TLCBot2.Core.CommandLine;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents;

public static class MessageComponentHandler
{
    public static List<FireMessageComponent> AllComponents = new();
    public static Task OnButtonExecuted(SocketMessageComponent button)
    {
        RuntimeConfig.CommandExecutionLog.SendMessageAsync(embed: new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("Button Executed")
            .AddField("Button's Original Message", button.Message.GetJumpUrl())
            .AddField("Button's ID", button.Data.CustomId)
            .WithDescription($"User: {button.User.Mention} : {button.User.Id}")
            .WithAuthor(button.User).Build());

        ClearComponentCache();
        if (!button.Data.CustomId.StartsWith("ETERNAL"))
        {
            if (AllComponents.Count == 0)
            {
                button.RespondAsync(
                                "This button has expired and can no longer be used. " +
                                    "This was done as an optimzation so the bot does not have " +
                                    "to worry about every single button in existence, and only " +
                                    "the relevant ones.", ephemeral: true);
                Helper.DisableMessageComponents(button.Message);
                return Task.CompletedTask;
            }
            foreach (var fireMessageComponent in AllComponents)
            {
                if (fireMessageComponent.Component.Components.Any(actionRow =>
                        actionRow.Components.Any(component =>
                            component.Type == ComponentType.Button
                            && component.CustomId == button.Data.CustomId)))
                {
                    if (fireMessageComponent.OwnerId == null || fireMessageComponent.OwnerId == button.User.Id)
                        fireMessageComponent.OnExecuteButton?.Invoke(button);
                    else button.RespondAsync("You are not responsible for this button", ephemeral: true);
                }
            }
        }
        else
        {
            EternalButtons.OnExecute(button);
        }
        return Task.CompletedTask;
    }
    public static Task OnSelectionMenuExecuted(SocketMessageComponent selectionMenu)
    {
        RuntimeConfig.CommandExecutionLog.SendMessageAsync(embed: new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("Selection Menu Executed")
            .AddField("Selection Menu's Original Message", selectionMenu.Message.GetJumpUrl())
            .AddField("Selected Value(s)", string.Join("\n", selectionMenu.Data.Values))
            .WithDescription($"User: {selectionMenu.User.Mention} : {selectionMenu.User.Id}")
            .WithAuthor(selectionMenu.User).Build());

        ClearComponentCache();
        if (!selectionMenu.Data.CustomId.StartsWith("ETERNAL"))
        {
            if (AllComponents.Count == 0)
            {
                selectionMenu.RespondAsync(
                    "This selection menu has expired and can no longer be used. " +
                    "This was done as an optimzation so the bot does not have " +
                    "to worry about every single button in existence, and only " +
                    "the relevant ones.", ephemeral: true);
                Helper.DisableMessageComponents(selectionMenu.Message);
                return Task.CompletedTask;
            }
            foreach (var fireMessageComponent in AllComponents)
            {
                if (fireMessageComponent.Component.Components.Any(actionRow =>
                        actionRow.Components.Any(component =>
                            component.Type == ComponentType.SelectMenu
                            && component.CustomId == selectionMenu.Data.CustomId)))
                {
                    if (fireMessageComponent.OwnerId == null || fireMessageComponent.OwnerId == selectionMenu.User.Id)
                        fireMessageComponent.OnExecuteButton?.Invoke(selectionMenu);
                    else selectionMenu.RespondAsync("You are not responsible for this selection menu", ephemeral: true);
                }
            }
        }
        else
        {
            EternalSelectMenus.OnExecute(selectionMenu);
        }
        return Task.CompletedTask;
    }

    public static void ClearComponentCache() => 
        AllComponents.RemoveAll(component => 
            DateTime.Now > component.BirthDate.Add(component.LifeTime));
}