using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;

namespace TLCBot2.ApplicationComponents;

public static class ModalHandler
{
    public static List<FireModal> AllModals = new();
    public static void Initialize()
    {
        
    }

    public static Task OnModalSubmitted(SocketModal modal)
    {
        for (int i = 0; i < AllModals.Count; i++)
        {
            if (AllModals[i].Modal.CustomId == modal.Data.CustomId)
            {
                AllModals[i].OnSubmitted?.Invoke(modal);
            }
        }
        return Task.CompletedTask;
    }
}