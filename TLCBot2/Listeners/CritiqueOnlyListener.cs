using Discord;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.Listeners;

public static class CritiqueOnlyListener
{
    public static Task OnMessageRecieved(SocketMessage message)
    {
        if (message.Channel.Id == RuntimeConfig.CritiqueMyWorkChannel.Id) // message is in critique my work
        {
            if (message.Attachments is {Count: 0}) // message is not image post
            {
                message.Author.SendMessageAsync(
                    $"Please do not speak dierctly in <#{RuntimeConfig.CritiqueMyWorkChannel.Id}>. " +
                    "Instead, you can give your critiques of people's artworks in the threads under their post.");
                message.DeleteAsync();
            }
            else
            {
                var thread = ((SocketTextChannel) message.Channel)
                    .CreateThreadAsync($"Critiques for {message.Author.Username}", message: message).Result;
                thread.LeaveAsync();
            }
        }
        return Task.CompletedTask;
    }
}