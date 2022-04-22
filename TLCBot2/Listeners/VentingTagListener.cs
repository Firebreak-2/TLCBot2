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
        if (matches.Count == 0 || matches.Any(x => x.Value.ToLower() == "nsfw") && matches.Count == 1)
        {
            message.Author.SendMessageAsync(
                $"The following message in {RuntimeConfig.VentingChannel.Mention} " +
                $"had no tags, and as a result was removed. If you want to use the {RuntimeConfig.VentingChannel.Mention} " +
                $"channel, you need to have at least one tag in it (excluding the nsfw tag). You can use the following tags, " +
                $"or any custom ones you think fit your vent\n" +
                $"`[HELP] - When you need help from others, whether it be life advice or needing other thoughts on a matter`\n" +
                $"`[VENT] - When you want to say something that's been bugging you and get it off your chest`\n" +
                $"`[NSFW] - Include this tag if your vent message contains NSFW content. In this case your message " +
                $"(outside of the tags) should be contained within spoilers. Example: \"[Nsfw] [Vent] ||Your Message.||\" " +
                $"To do you this you surround your message with two \"|\" symbols, afterwhich discord will make it " +
                $"not visible until clicked.`\n```{message.Content}```");
            message.DeleteAsync();
            return Task.CompletedTask;
        }
        var tags = matches.Select(x => x.Value);
        if (tags.Contains("nsfw")) // if text outside of tags is not spoilered, repost message as bot and spoiler text
        {
            var tagWithBracketsRegex = new Regex(@"\[(.+?)\]");
            string taglessMessage = tagWithBracketsRegex.Replace(message.Content, "").Trim();
            if (Regex.Replace(taglessMessage, @"\s+|\|+", "").Length > 0)
                message.Channel.SendMessageAsync(
                    $"{string.Join(" ", tags.Select(x => $"[{x.ToUpper()}]"))}, Post by {message.Author.Mention}\n" +
                    $"||{taglessMessage.Replace("|", "")}||",
                    allowedMentions: AllowedMentions.None);
            message.DeleteAsync();
        }
        if (tags.Contains("help"))
            ((SocketTextChannel) message.Channel).CreateThreadAsync($"Help {message.Author.Username}",
                message: message).Result.LeaveAsync();
        
        return Task.CompletedTask;
    }
}