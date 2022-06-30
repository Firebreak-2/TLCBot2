using System.Reflection;
using Discord.Interactions;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands : InteractionModuleBase<SocketInteractionContext>
{
    public static readonly InteractionService Service = new(Program.Client);

    [PreInitialize]
    public static Task PreInitialize()
    {
        Program.Client.InteractionCreated += async interaction =>
        {
            var ctx = new SocketInteractionContext(Program.Client, interaction);
            try
            {
                await Service.ExecuteCommandAsync(ctx, null);
            }
            catch (Exception e)
            {
                await interaction.RespondAsync($"```{e}```", ephemeral: true);
            }
        };

        return Task.CompletedTask;
    }

    [Initialize]
    public static async Task Initialize()
    {
        await Service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        await Service.RegisterCommandsToGuildAsync(RuntimeConfig.FocusServer!.Id);
    }
}