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
        ModalHandler.AllModals.RemoveAll(item =>
            item.Modal.CustomId == Modal.CustomId);
        
        ModalHandler.AllModals.Add(this);
        return Modal;
    }
}