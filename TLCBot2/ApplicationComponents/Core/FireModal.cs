using Discord;
using Discord.WebSocket;

namespace TLCBot2.ApplicationComponents.Core;

public class FireModal
{
    public Modal Modal;
    public ModalBuilder Builder;
    public Action<SocketModal>? OnSubmitted;

    public FireModal(
        ModalBuilder modal,
        Action<SocketModal> onSubmitted)
    {
        Builder = modal;
        Modal = modal.Build();
        OnSubmitted = onSubmitted;
    }
    public static Modal CreateNew(FireModal command)
    {
        return command.Create();
    }
    public Modal Create()
    {
        bool Condition(FireModal x) =>
            x.Modal.CustomId == Modal.CustomId;
        
        if (ModalHandler.AllModals.Any() && 
            ModalHandler.AllModals.Any(Condition))
            ModalHandler.AllModals.RemoveAll(Condition);
        
        ModalHandler.AllModals.Add(this);
        return Modal;
    }
}