using Discord;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.ApplicationComponents.Core;

public class FireMessageComponent
{
    public MessageComponent Component;
    public ComponentBuilder Builder;
    public Action<SocketMessageComponent>? OnExecuteButton;
    public Action<SocketMessageComponent>? OnExecuteSelectMenu;
    public DateTime BirthDate = DateTime.Now;
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
    public static MessageComponent CreateNew(FireMessageComponent command)
    {
        return command.Create();
    }
    public MessageComponent Create()
    {
        bool Condition(FireMessageComponent x) => 
            x.Component.Components.FirstOrDefault()!
                .Components.FirstOrDefault()!.CustomId ==
                Component.Components.FirstOrDefault()!
                    .Components.FirstOrDefault()!.CustomId;
        
        if (MessageComponentHandler.AllComponents.Any() && 
            MessageComponentHandler.AllComponents.Any(Condition))
            MessageComponentHandler.AllComponents.RemoveAll(Condition);
        
        MessageComponentHandler.AllComponents.Add(this);
        return Component;
    }
}