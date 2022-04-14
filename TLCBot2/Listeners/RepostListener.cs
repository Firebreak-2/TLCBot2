using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Listeners;

public static class RepostListener
{
    public static Task OnMessageReceived(SocketMessage message)
    {
        if (RuntimeConfig.WhitelistedStarboardChannels.All(x => x.Id != message.Channel.Id)) return Task.CompletedTask;
        bool hasUrl = Helper.HasUrl(message.Content, out var url);
        if (message.Attachments is {Count: 0} && !hasUrl) return Task.CompletedTask;

        using var client = new WebClient();
        string content = "";

        void AddWithLink(string imageLink)
        {
            string address = "https://yandex.com/images/search?family=yes&rpt=imageview&url=" + imageLink;
            string downloadedPage = client.DownloadString(address);
            if (Regex.IsMatch(downloadedPage, @"(?<=<div class=""CbirOtherSizes-EmptyMessage"">)No matching images found(?=</div>)"))
                content += $"No matching images found for {imageLink}";
            else
            {
                const int limit = 4;
                var avatars = Regex.Matches(
                    downloadedPage, 
                    @"(?<=<img class=""MMImage Thumb-Image"" src="").+?(?="" alt="".+?"" width="".+?"" height="".+?"">)"
                ).Select(x => $"https:{x.Value}").Where((_, i) => i < limit).ToArray();
                var links = Regex.Matches(
                    downloadedPage, 
                    @"(?<=<a href="")https:.+?(?="" target="".+?"" class=""Link Link_theme_normal"">.+?<\/a>)"
                ).Select(x => $"[<{x.Value}>]").Where((_, i) => i < limit).ToArray();

                int length = avatars.Length;
                content += $"{length} Matching images found for {imageLink}";
                for (int i = 0; i < avatars.Length; i++)
                {
                    content += $"  {avatars[i]}\n  {links[i]}\n";
                }
            }
        }
        if (hasUrl)
            AddWithLink(url!);
        foreach (var attachment in message.Attachments)
        {
            AddWithLink(attachment.Url);
        }

        RuntimeConfig.BotReportsChannel.SendMessageAsync(content + $"\n{message.GetJumpUrl()}");
        
        return Task.CompletedTask;
    }
}