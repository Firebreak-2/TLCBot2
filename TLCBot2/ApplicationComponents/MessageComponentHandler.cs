using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;

namespace TLCBot2.ApplicationComponents;

public static class MessageComponentHandler
{
    public static List<FireMessageComponent> AllComponents = new();
    public static void Initialize()
    {
        
    }
    public static Task OnButtonExecuted(SocketMessageComponent button)
    {
        foreach (var component in AllComponents
                     .Where(component => component.Component.Components.First()
                         .Components.Any(x => x.CustomId == button.Data.CustomId)))
        {
            component.OnExecuteButton?.Invoke(button);
        }
        return Task.CompletedTask;
    }
    public static Task OnSelectionMenuExecuted(SocketMessageComponent selectionMenu)
    {
        for (int i = 0; i < AllComponents.Count; i++)
        {
            if (AllComponents[i].Component.Components.First()
                .Components.Any(x => x.CustomId == selectionMenu.Data.CustomId))
            {
                AllComponents[i].OnExecuteSelectMenu?.Invoke(selectionMenu);
            }
        }
        return Task.CompletedTask;
    }
}