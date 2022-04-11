using Discord;
using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Utilities;
using static TLCBot2.ApplicationComponents.MessageComponentHandler;

namespace TLCBot2.ApplicationComponents.Core;

public class FireMessageComponent
{
    public MessageComponent Component;
    public ComponentBuilder Builder;
    public Action<SocketMessageComponent>? OnExecuteButton;
    public Action<SocketMessageComponent>? OnExecuteSelectMenu;
    public ulong? OwnerId = null;
    public DateTime BirthDate = DateTime.Now;
    public TimeSpan LifeTime = TimeSpan.FromMinutes(5);
    public FireMessageComponent(
        ComponentBuilder component,
        Action<SocketMessageComponent>? onExecuteButton,
        Action<SocketMessageComponent>? onExecuteSelectMenu)
    {
        Builder = component;
        Component = component.Build();
        OnExecuteButton = onExecuteButton;
        OnExecuteSelectMenu = onExecuteSelectMenu;
    }
    public static MessageComponent CreateNew(FireMessageComponent command) =>
        command.Create();
    public MessageComponent Create()
    {
        AllComponents.RemoveAll(item =>
            item.Component.Components.Any(actionRow =>
                actionRow.Components.Any(messageComponent => 
                    Component.Components.Any(thisActionRow =>
                        thisActionRow.Components.Any(thisMessageComponent =>
                            thisMessageComponent.CustomId == messageComponent.CustomId)))));
        
        AllComponents.Add(this);
        return Component;
    }
}