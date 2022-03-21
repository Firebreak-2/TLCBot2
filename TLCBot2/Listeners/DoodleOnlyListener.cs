using Discord;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.Listeners;

public static class DoodleOnlyListener
{
    public static Task OnMessageRecieved(SocketMessage message)
    {
        if (message.Channel.Id == RuntimeConfig.DoodleOnlyChannel.Id && message.Attachments.Count == 0)
        {
            message.Author.SendMessageAsync(
                $"Please do not speak in <#{RuntimeConfig.DoodleOnlyChannel.Id}>." +
                "The channel is meant for people to speak only through drawings and doodles, not text.");
            message.DeleteAsync();
        }
        return Task.CompletedTask;
    }
}