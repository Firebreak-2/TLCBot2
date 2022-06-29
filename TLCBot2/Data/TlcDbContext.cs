using Microsoft.EntityFrameworkCore;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;

namespace TLCBot2.Data;

public class TlcDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={Program.FileAssetsPath}/TLC.db");
    
    public DbSet<ProfileEntry> Users { get; set; }
    public DbSet<ChannelSettingsEntry> ChannelSettings { get; set; }

    [PreInitialize]
    public static async Task Initialize()
    {
        await using var db = new TlcDbContext();
        await db.Database.EnsureCreatedAsync();
        await db.SaveChangesAsync();
    }
}