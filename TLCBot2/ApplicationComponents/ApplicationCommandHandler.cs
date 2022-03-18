using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Commands.MessageCommands;
using TLCBot2.ApplicationComponents.Commands.SlashCommands;
using TLCBot2.ApplicationComponents.Commands.UserCommands;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationComponents;

public static class ApplicationCommandHandler
{
    public static List<FireSlashCommand> AllSlashCommands = new();
    public static List<FireUserCommand> AllUserCommands = new();
    public static List<FireMessageCommand> AllMessageCommands = new();
    public static async Task Initialize()
    {
        await TestSlashCommands.Initialize();
        await CommercialSlashCommands.Initialize();
        await TestUserCommands.Initialize();
        // await CommercialUserCommands.Initialize();
        // await TestMessageCommands.Initialize();
        // await CommercialMessageCommands.Initialize();

        var allCommands = AllSlashCommands
            .Select(x => x.Slashie)
            .Union<ApplicationCommandProperties>(AllUserCommands
                .Select(x => x.Command))
            .Union(AllMessageCommands
                .Select(x => x.Command));

        await Constants.Guilds.Lares!.BulkOverwriteApplicationCommandAsync(allCommands.ToArray());
        
        // if (allCommands.Any(x => x.Guild == null))
        //     await Program.Client.BulkOverwriteGlobalApplicationCommandsAsync(allCommands
        //         .Where(x => x.Guild == null).ToArray());
    }
}