using Discord.Interactions;
using TLCBot2.Data.StringPrompts;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [ComponentInteraction("dashboard-directory-select-menu")]
    public async Task DashboardDirectorySelectMenuResponse(string[] selectedOptions)
    {
        switch (selectedOptions[0])
        {
            case "h": // hangout
            {
                await RespondAsync(StringPrompts.DashboardDirectoryHangout, ephemeral: true);
                break;
            }
            case "s": // share
            {
                await RespondAsync(StringPrompts.DashboardDirectoryShare, ephemeral: true);
                break;
            }
            case "i": // improve & support
            {
                await RespondAsync(StringPrompts.DashboardDirectoryImprove, ephemeral: true);
                break;
            }
        }
    }
}