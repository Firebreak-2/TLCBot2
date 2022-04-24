using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.Listeners;

public static class VentingTagListener
{
    public static Task OnMessageReceived(SocketMessage message)
    { 
        if (message.Channel.Id != RuntimeConfig.VentingChannel.Id || message.Author.IsBot) return Task.CompletedTask;

        var tagRegex = new Regex(@"(?<=\[)(.+?)(?=\])");
        var matches = tagRegex.Matches(message.Content.ToLower());
        if (matches.Count == 0)
        {
            message.Author.SendMessageAsync(
                $"The following message in {RuntimeConfig.VentingChannel.Mention} " +
                $"had no tags, and as a result was removed. If you want to use the {RuntimeConfig.VentingChannel.Mention} " +
                $"channel, you need to have at least one tag in it. You can use the following tags, " +
                $"or any custom ones you think fit your vent\n" +
                $"`[HELP] - When you need help from others, whether it be life advice or needing other thoughts on a matter`\n" +
                $"`[VENT] - When you want to say something that's been bugging you and get it off your chest`\n" +
                $"```{message.Content}```");
            message.DeleteAsync();
            return Task.CompletedTask;
        }
        var tags = matches.Select(x => x.Value);
        if (tags.Contains("help"))
            ((SocketTextChannel) message.Channel).CreateThreadAsync($"Help {message.Author.Username}",
                message: message).Result.LeaveAsync();
        
        return Task.CompletedTask;
    }
}