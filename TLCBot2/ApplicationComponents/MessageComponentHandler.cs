using System.ComponentModel;
using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.ApplicationComponents.Eternal;

namespace TLCBot2.ApplicationComponents;

public static class MessageComponentHandler
{
    public static List<FireMessageComponent> AllComponents = new();
    public static Task OnButtonExecuted(SocketMessageComponent button)
    {
        ClearComponentCache();
        if (!button.Data.CustomId.StartsWith("ETERNAL"))
        {
            for (int i = 0; i < AllComponents.Count; i++)
            {
                if (AllComponents[i].Component.Components.First()
                    .Components.Any(x => x.CustomId == button.Data.CustomId))
                {
                    AllComponents[i].OnExecuteButton?.Invoke(button);
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
        ClearComponentCache();
        if (!selectionMenu.Data.CustomId.StartsWith("ETERNAL"))
        {
            for (int i = 0; i < AllComponents.Count; i++)
            {
                if (AllComponents[i].Component.Components.First()
                    .Components.Any(x => x.CustomId == selectionMenu.Data.CustomId))
                {
                    AllComponents[i].OnExecuteSelectMenu?.Invoke(selectionMenu);
                }
            }
        }
        else
        {
            EternalSelectMenus.OnExecute(selectionMenu);
        }
        return Task.CompletedTask;
    }

    public static void ClearComponentCache()
    {
        AllComponents
            .RemoveAll(component =>
                DateTime.Now > component.BirthDate.AddMinutes(5));
    }
}