using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;

namespace TLCBot2.ApplicationComponents;

public static class ModalHandler
{
    public static List<FireModal> AllModals = new();
    public static Task OnModalSubmitted(SocketModal modal)
    {
        for (int i = 0; i < AllModals.Count; i++)
        {
            if (AllModals[i].Modal.CustomId != modal.Data.CustomId) continue;
            
            AllModals[i].OnSubmitted?.Invoke(modal);
            AllModals.Remove(AllModals[i]);
            break;
        }
        return Task.CompletedTask;
    }
}