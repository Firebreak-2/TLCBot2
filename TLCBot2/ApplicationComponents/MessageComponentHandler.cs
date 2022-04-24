using System.ComponentModel;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
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
        ClearComponentCache();
        if (!button.Data.CustomId.StartsWith("ETERNAL"))
        {
            bool Condition(FireMessageComponent fireMessageComponent) =>
                fireMessageComponent.Component.Components.Any(actionRow =>
                    actionRow.Components.Any(component =>
                        component.Type == ComponentType.Button
                        && component.CustomId == button.Data.CustomId));
            
            if (AllComponents.Any(Condition))
            {
                var messageComponent = AllComponents.First(Condition);
                if (messageComponent.OwnerId == null || messageComponent.OwnerId == button.User.Id)
                {
                    try
                    {
                        messageComponent.OnExecuteButton?.Invoke(button);
                    }
                    catch (Exception e)
                    {
                        Helper.LogInteractionError($"{JsonConvert.SerializeObject(e, Formatting.Indented)}", "button", button.Message);
                        button.RespondAsync(
                            "Uh oh, something failed. The development team has been notified of the error.",
                            ephemeral: true);
                    }
                }
                else 
                    button.RespondAsync("You are not responsible for this button", ephemeral: true);
            }
            else
            {
                button.RespondAsync(
                    "This button has expired and can no longer be used. " +
                    "This was done as an optimzation so the bot does not have " +
                    "to worry about every single button in existence, and only " +
                    "the relevant ones.", ephemeral: true);
                Helper.DisableMessageComponents(button.Message);
                return Task.CompletedTask;
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
        ClearComponentCache();
        if (!selectionMenu.Data.CustomId.StartsWith("ETERNAL"))
        {
            bool Condition(FireMessageComponent fireMessageComponent) =>
                fireMessageComponent.Component.Components.Any(actionRow =>
                    actionRow.Components.Any(component =>
                        component.Type == ComponentType.SelectMenu
                        && component.CustomId == selectionMenu.Data.CustomId));

            if (AllComponents.Any(Condition))
            {
                var messageComponent = AllComponents.First(Condition);
                if (messageComponent.OwnerId == null || messageComponent.OwnerId == selectionMenu.User.Id)
                {
                    try
                    {
                        messageComponent.OnExecuteSelectMenu?.Invoke(selectionMenu);
                    }
                    catch (Exception e)
                    {
                        Helper.LogInteractionError($"{JsonConvert.SerializeObject(e, Formatting.Indented)}", "select menu", selectionMenu.Message);
                        selectionMenu.RespondAsync(
                            "Uh oh, something failed. The development team has been notified of the error.",
                            ephemeral: true);
                    }
                }
                else 
                    selectionMenu.RespondAsync("You are not responsible for this selection menu", ephemeral: true);
            }
            else
            {
                selectionMenu.RespondAsync(
                    "This selection menu has expired and can no longer be used. " +
                    "This was done as an optimzation so the bot does not have " +
                    "to worry about every single button in existence, and only " +
                    "the relevant ones.", ephemeral: true);
                Helper.DisableMessageComponents(selectionMenu.Message);
                return Task.CompletedTask;
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