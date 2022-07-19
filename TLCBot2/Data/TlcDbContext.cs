using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;

namespace TLCBot2.Data;

public class TlcDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={Program.FileAssetsPath}/TLC.db");

    public DbSet<ServerInviteEntry> ServerInvites { get; set; }
    public DbSet<LogEntry> Logs { get; set; }
    public DbSet<ProfileEntry> Users { get; set; }
    public DbSet<ActiveUserEntry> ActiveUsers { get; set; }
    public DbSet<ChannelSettingsEntry> ChannelSettings { get; set; }

    [PreInitialize]
    public static async Task Initialize()
    {
        await using var db = new TlcDbContext();
        await db.Database.EnsureCreatedAsync();
        await db.SaveChangesAsync();
    }
}