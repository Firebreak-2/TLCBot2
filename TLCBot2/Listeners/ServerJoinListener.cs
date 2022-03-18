﻿using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using TLCBot2.Core;
using TLCBot2.DataManagement;

namespace TLCBot2.Listeners;

public static class ServerJoinListener
{
    public static string JoinHistoryPath => $"{Program.FileAssetsPath}\\serverInvitedHistory.json";
    public static string PreviousInvites => $"{Program.FileAssetsPath}\\oldInvites.txt";
    public static InviteData[] OldInvites
    {
        get => JsonConvert.DeserializeObject<InviteData[]>(File.ReadAllText(JoinHistoryPath))!;
        set => File.WriteAllText(JoinHistoryPath, JsonConvert.SerializeObject(value, Formatting.Indented));
    }
    public static void Initialize()
    {
        if (!File.Exists(JoinHistoryPath))
            File.WriteAllText(JoinHistoryPath, "[]");
        if (!File.Exists(PreviousInvites))
            File.WriteAllText(PreviousInvites, "");
    }
    public static Task OnMemberJoined(SocketGuildUser user)
    {
        var oldInvites = OldInvites;
        foreach (var item in user.Guild.GetInvitesAsync().Result)
        {
            if (item.Uses <= oldInvites.First(x => x.Id == item.Id).Uses && !item.Inviter.IsBot) continue;
            
            string invitation = $"<@!{item.Inviter.Id}> Thanks for inviting <@!{user.Id}> to the server.";
            if (File.ReadAllLines(PreviousInvites).Any(x => x == invitation)) break;
            
            CookieManager.TakeOrGiveCookiesToUser(item.Inviter.Id, 3, 
                $"Invited [{user.Username}] to server\nInviter: {item.Inviter.Mention} | {item.Inviter.Username}#{item.Inviter.Discriminator} | {item.Inviter.Id}\nInvited: {user.Mention} | {user.Username}#{user.Discriminator} | {user.Id}");
            ((SocketTextChannel)RuntimeConfig.GeneralChat).SendMessageAsync(invitation + " Have 3 🍪");
            File.AppendAllText(PreviousInvites, invitation);
            break;
        }
        return Task.CompletedTask;
    }
    public static Task OnInviteCreated(SocketInvite invite)
    {
        OldInvites = invite.Guild.GetInvitesAsync().Result.Select(x => new InviteData(x.Id, x.Uses)).ToArray();
        return Task.CompletedTask;
    }

    public record InviteData(string Id, int? Uses)
    {
        public readonly string Id = Id;
        public int? Uses = Uses;
    }
}