using Microsoft.EntityFrameworkCore;
using TLCBot2.Attributes;
using TLCBot2.Data;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "Directly runs a SQL expression into the database. \x1b[2;31mVERY dangerous.")]
    public static async Task DbRun(string sqlExpression)
    {
        await using var db = new TlcDbContext();
        int rowsAffected = await db.Database.ExecuteSqlRawAsync(sqlExpression);
        await ChannelTerminal.PrintAsync($"{sqlExpression}\n```\n```\nNumber of rows affected: {rowsAffected}", "sql");
    }
}