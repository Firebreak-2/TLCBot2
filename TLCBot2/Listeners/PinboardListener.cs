using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using TLCBot2.Core;
using TLCBot2.Utilities;
using Image = SixLabors.ImageSharp.Image;

namespace TLCBot2.Listeners;

public static class PinboardListener
{
    public static Task OnMessageReceived(SocketMessage message)
    {
        if (message.Channel.GetGuild().Id != RuntimeConfig.FocusServer.Id || message.Reference == null)
            return Task.CompletedTask;
        
        var reference = (IUserMessage)message.Channel.GetMessageAsync(message.Reference.MessageId.Value).Result;
        if (!reference.IsPinned) return Task.CompletedTask;
        
        reference.UnpinAsync();
        // RuntimeConfig.PinboardChannel.SendMessageAsync(reference.Content);
        RestWebhook hook;

        string name = RuntimeConfig.PinboardChannel.Guild.GetUser(reference.Author.Id)?.Nickname ??
                      reference.Author.Username;
        if (RuntimeConfig.PinboardChannel.GetWebhooksAsync().Result is {Count: > 0} webhooks)
        {
            hook = webhooks.First();
            // hook.ModifyAsync(props =>
            // {
            //     if (hook.Name != name)
            //         props.Name = name;
            //     if (hook.AvatarId != reference.Author.AvatarId)
            //     {
            //         string url = reference.Author.GetAvatarUrl() ?? reference.Author.GetDefaultAvatarUrl()!;
            //         using var client = new WebClient();
            //         byte[] data = client.DownloadData(new Uri(url));
            //         Stream stream = Image.Load(data).ToStream();
            //         props.Image = new Discord.Image(stream);
            //     }
            // });
        }
        else
            hook = 
                RuntimeConfig.PinboardChannel.CreateWebhookAsync(
                    name).Result;
        
        using var client = new DiscordWebhookClient(hook);
        
        var messageData = (
            text: reference.Content,
            isTTS: false,
            embeds: reference.Embeds.Select(x => (Embed) x).Append(new EmbedBuilder()
                .WithTitle($"New Message Pinned by {message.Author.Username}")
                .WithDescription($"Original message in <#{message.Channel.Id}>: {reference.GetJumpUrl()}").Build())
                .Where(x => !string.IsNullOrEmpty(x.Title) || !string.IsNullOrEmpty(x.Description)),
            username: name,
            avatarUrl: reference.Author.GetAvatarUrl() ?? reference.Author.GetDefaultAvatarUrl(),
            options: RequestOptions.Default,
            allowedMentions: AllowedMentions.None,
            attachments: reference.Attachments
                .Where(x => Regex.IsMatch(x.Url, 
                    @".+\.(?:TGA|JPEG|GIF|Webp|PNG|BMP|TIFF|PBM)$",
                    RegexOptions.IgnoreCase))
                .Select(x =>
                {
                    using var stream = Helper.ImageFromUrl(x.Url).ToStream();
                    return new FileAttachment(stream, x.Filename, x.Description,
                        x.IsSpoiler());
                }));
        
        if (reference.Attachments is null or {Count: 0})
            client.SendMessageAsync(
                messageData.text,
                messageData.isTTS,
                messageData.embeds,
                messageData.username,
                messageData.avatarUrl,
                messageData.options,
                messageData.allowedMentions);
        else 
            client.SendFilesAsync(
                messageData.attachments,
                messageData.text,
                messageData.isTTS,
                messageData.embeds,
                messageData.username,
                messageData.avatarUrl,
                messageData.options,
                messageData.allowedMentions);
        

        return Task.CompletedTask;
    } 
}