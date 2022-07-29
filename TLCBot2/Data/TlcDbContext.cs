using Microsoft.EntityFrameworkCore;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Types;

namespace TLCBot2.Data;

public class TlcDbContext : DbContext
{
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     string DB_USER = Environment.GetEnvironmentVariable("DB_USER") 
    //         ?? throw new Exception("DB_USER env variable not found");
    //     string DB_PASSWORD = Environment.GetEnvironmentVariable("DB_PASSWORD") 
    //         ?? throw new Exception("DB_PASSWORD env variable not found");
    //     string DB_NAME = Environment.GetEnvironmentVariable("DB_NAME") 
    //         ?? throw new Exception("DB_NAME env variable not found");
    //         
    //     optionsBuilder.UseSqlite($"Data Source=psql://{DB_USER}:{DB_PASSWORD}@db:5432/{DB_NAME}");
    // }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={Program.FileAssetsPath}/TLC.db");

    public DbSet<ServerInviteEntry> ServerInvites { get; set; }
    public DbSet<LogEntry> Logs { get; set; }
    public DbSet<ProfileEntry> Users { get; set; }
    public DbSet<ActiveUserEntry> ActiveUsers { get; set; }
    // public DbSet<ChannelSettingsEntry> ChannelSettings { get; set; }
    public DbSet<EventLogActionEntry> EventLogActions { get; set; }

    [PreInitialize]
    public static async Task Initialize()
    {
        await using var db = new TlcDbContext();
        await db.Database.EnsureCreatedAsync();
        await db.SaveChangesAsync();
    }
}