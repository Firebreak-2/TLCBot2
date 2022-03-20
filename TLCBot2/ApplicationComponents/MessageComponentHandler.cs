using System.ComponentModel;
using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.ApplicationComponents.Eternal;
using TLCBot2.Core;

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
            .WithDescription($"User: {button.User.Mention} : {button.User.Id}")
            .WithAuthor(button.User).Build());

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

    public static void ClearComponentCache() => 
        AllComponents.RemoveAll(component => 
            DateTime.Now > component.BirthDate.AddMinutes(5));
}