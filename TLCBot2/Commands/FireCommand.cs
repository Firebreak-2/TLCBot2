using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Commands;

public class FireCommand
{
    public SlashCommandProperties Slashie;
    public Action<SocketSlashCommand> OnExecute;
    public bool DevOnly;
    public SocketGuild? Guild = null;
    public FireCommand(SlashCommandBuilder slashie, Action<SocketSlashCommand> onExecute, bool devOnly = false)
    {
        Slashie = slashie.Build();
        OnExecute = onExecute;
        DevOnly = devOnly;
    }

    public static async Task CreateNew(FireCommand command, SocketGuild? guild)
    {
        await command.Create(guild);
    }
    public Task Create(SocketGuild? guild)
    {
        Guild = guild;
        CommandHandler.AllCommands.Add(this);
        return Task.CompletedTask;
        // try
        // {
        //     if (guild != null)
        //     {
        //         Program.InitApplicationCommandProperties.Add(Slashie);
        //     }
        //     else
        //         await Constants.Guilds.Lares!.CreateApplicationCommandAsync(Slashie);
        // }
        // catch (HttpException exception)
        // {
        //     string json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
        //     Console.WriteLine(json);
        // }
    }
}