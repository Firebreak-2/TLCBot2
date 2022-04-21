using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Listeners;

public static class ServerStatsListener
{
    public static void Initialize()
    {
        UpdateMemberCount();
        UpdateDaysSinceOpen();
    }
    public static Task OnMemberJoined(SocketGuildUser user)
    {
        UpdateMemberCount();
        return Task.CompletedTask;
    }
    public static Task OnMemberLeft(SocketGuild guild, SocketUser user)
    {
        UpdateMemberCount();
        return Task.CompletedTask;
    }
    public static void UpdateMemberCount()
    {
        RuntimeConfig.StatMembersVC.ModifyAsync(props =>
        {
            props.Name = $"Member Count: {RuntimeConfig.StatMembersVC.Guild.MemberCount}";
        });
    }
    public static void UpdateDaysSinceOpen()
    {
        RuntimeConfig.StatDaysActiveVC.ModifyAsync(props =>
        {
            props.Name = $"Days Open: {DateTime.Now.Subtract(RuntimeConfig.StatMembersVC.Guild.CreatedAt.DateTime).Days}";
        });
    }
}