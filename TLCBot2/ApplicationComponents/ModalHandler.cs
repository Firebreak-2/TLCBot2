using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;

namespace TLCBot2.ApplicationComponents;

public static class ModalHandler
{
    public static List<FireModal> AllModals = new();
    public static Task OnModalSubmitted(SocketModal modal)
    {
        foreach (var fireModal in AllModals)
        {
            if (fireModal.Modal.CustomId != modal.Data.CustomId) continue;
            
            fireModal.OnSubmitted?.Invoke(modal);
            AllModals.Remove(fireModal);
            break;
        }
        return Task.CompletedTask;
    }
}