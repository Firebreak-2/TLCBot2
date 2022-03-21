using Discord;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.Listeners;

public static class AutoThreadListener
{
    public static Task OnMessageRecieved(SocketMessage message)
    {
        if (message.Channel.Id == RuntimeConfig.QOTDChannel.Id)
        {
            ((SocketTextChannel) message.Channel).CreateThreadAsync(DateTime.UtcNow.ToShortDateString(), message: message);
        }
        return Task.CompletedTask;
    }
}