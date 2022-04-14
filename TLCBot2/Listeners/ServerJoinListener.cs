using System.Text.RegularExpressions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using TLCBot2.Core;
using TLCBot2.DataManagement;
using TLCBot2.DataManagement.Cookies;

namespace TLCBot2.Listeners;

public static class ServerJoinListener
{
    public static string JoinHistoryPath => $"{Program.FileAssetsPath}/serverInvitedHistory.json";
    public static string PreviousInvites => $"{Program.FileAssetsPath}/oldInvites.txt";
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
        if (DateTime.Now.Subtract(TimeSpan.FromDays(30 * 6)) < user.CreatedAt)
            RuntimeConfig.BotReportsChannel.SendMessageAsync(text:
                $"New member <@!{user.Id}>'s account is less than 6 months old\n" +
                $"created <t:{user.CreatedAt.ToUnixTimeMilliseconds()}:F>");
        
        var oldInvites = OldInvites;
        foreach (var item in user.Guild.GetInvitesAsync().Result)
        {
            bool Condition(InviteData x) => x.Id == item!.Id;
            if (oldInvites.Any(Condition) && item.Uses <= oldInvites.First(Condition).Uses && !item.Inviter.IsBot) continue;

            string inviterString = $"<@!{item.Inviter.Id}> Thanks for inviting";
            string invitation = $"{inviterString} <@!{user.Id}> to the server.";
            if (File.ReadAllLines(PreviousInvites).Any(x => x == invitation)) break;
            
            CookieManager.TakeOrGiveCookiesToUser(item.Inviter.Id, 3, 
                $"Invited [{user.Username}] to server\nInviter: {item.Inviter.Mention} | {item.Inviter.Username}#{item.Inviter.Discriminator} | {item.Inviter.Id}\nInvited: {user.Mention} | {user.Username}#{user.Discriminator} | {user.Id}");
            RuntimeConfig.GeneralChat.SendMessageAsync(invitation + " Have 3 🍪");
            File.AppendAllText(PreviousInvites, $"{invitation}\n");

            var inviter = RuntimeConfig.FocusServer.GetUser(item.Inviter.Id);
            if (Regex.Matches(File.ReadAllText(PreviousInvites), inviterString).Count >= 5 
                && inviter.Roles.All(x => x.Id != RuntimeConfig.SecretsRole.Id))
                inviter.AddRoleAsync(RuntimeConfig.SecretsRole);
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