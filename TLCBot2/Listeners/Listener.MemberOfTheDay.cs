using Discord.WebSocket;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Types;

namespace TLCBot2.Listeners;

public static partial class Listener
{
    // executes once per hour
    [TimedEvent(1000 * 60 * 60)]
    public static async Task MemberOfTheDay()
    {
        if (RuntimeConfig.MotdCycleEpochTime == 0)
            RuntimeConfig.MotdCycleEpochTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        if (DateTimeOffset.FromUnixTimeSeconds(RuntimeConfig.MotdCycleEpochTime) + 24.Hours() > DateTimeOffset.Now)
        {
            RuntimeConfig.MotdCycleEpochTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            await HandleMemberOfTheDay();
        }
    }

    public static async Task HandleMemberOfTheDay()
    {
        var guild = RuntimeConfig.FocusServer!;
        if (RuntimeConfig.MotdRole is not { } role)
            return;
        
        foreach (var member in role.Members)
        {
            await member.RemoveRoleAsync(role);
        }

        await using var db = new TlcDbContext();

        var mostActive = db.ActiveUsers
            .OrderByDescending(x => x.LatestMessageCount)
            .ToArray()
            .Where(x => !RuntimeConfig.FocusServer!.GetUser(x.UserId).Roles.Contains(RuntimeConfig.CantParticipateInMotdRole));

        if (!mostActive.Any())
            return;
        
        await guild.GetUser(mostActive.First().UserId).AddRoleAsync(role);

        await db.Database.ExecuteSqlRawAsync(
            @"DELETE FROM ActiveUsers WHERE TRUE");

        await db.SaveChangesAsync();
    }

    [PreInitialize]
    public static async Task OnMessageReceived() =>
        Program.Client.MessageReceived += async message =>
        {
            await using var db = new TlcDbContext();
            if (await db.ActiveUsers.FindAsync(message.Author.Id) is { } entry)
                entry.LatestMessageCount++;
            else 
                await db.ActiveUsers.AddAsync(new ActiveUserEntry(message.Author.Id));
        };
}