using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.CommandLine;
using TLCBot2.CommandLine.Commands;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Logging;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [ComponentInteraction("logfig-button;*")]
    public async Task LogChannelConfig(string action)
    {
        switch (action)
        {
            case "cc": // Change Log Channel
            {
                await RespondWithModalAsync<ChangeLogChannelModal>("logfig-cc-modal");
                break;
            }
            case "cf": // Change Filter Settings
            {
                await Context.Interaction.RespondWithModalAsync<ModifyLoggingFiltersModal>("logfig-cf-modal",
                    modifyModal: modal =>
                    {
                        modal.Components = new ModalComponentBuilder()
                            .WithTextInput("Enter Filtered Tags", "new-tags", required: false,
                                value: string.Join(", ", RuntimeLogConfig.IgnoredLogTags))
                            .WithTextInput("Enter Filtered Importance Values", "new-importances", required: false,
                                value: string.Join(", ", RuntimeLogConfig.IgnoredLogImportances.Select(x => x.ToString())))
                            .WithTextInput("Enter Filtered Events By Name", "new-names", required: false,
                                value: string.Join(", ", RuntimeLogConfig.IgnoredLogEvents));
                    });
                break;
            }
            case "rf": // Restore Filter Settings
            {
                RuntimeLogConfig.IgnoredLogEvents.Clear();
                RuntimeLogConfig.IgnoredLogTags.Clear();
                RuntimeLogConfig.IgnoredLogImportances.Clear();
                
                RuntimeLogConfig.IgnoredLogImportances.Add(Log.Importance.Useless);
                await RuntimeLogConfig.SaveAllAsync();

                await RespondAsync("Restored default settings", ephemeral: true);
                break;
            }
            default:
                throw new NotImplementedException();
        }
    }

    [ModalInteraction("logfig-cc-modal")]
    public async Task ChangeLogChannelModalResponse(ChangeLogChannelModal modal)
    {
        if (!ulong.TryParse(modal.NewChannelId, out ulong id))
        {
            await RespondAsync("Provided ID value is not a number",
                ephemeral: true);
            return;
        }

        await TerminalCommands.SetConfig(nameof(RuntimeConfig.ServerLogsChannel), modal.NewChannelId);
        
        await RespondAsync($"Changed Server Log Channel. New Channel: <#{modal.NewChannelId}>",
            ephemeral: true);
    }

    [ModalInteraction("logfig-cf-modal")]
    public async Task ModifyLoggingFiltersModalResponse(ModifyLoggingFiltersModal modal)
    {
        RuntimeLogConfig.IgnoredLogEvents = modal.NewNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();
        RuntimeLogConfig.IgnoredLogTags = modal.NewTags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();
        RuntimeLogConfig.IgnoredLogImportances = modal.NewImportances
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Enum.Parse<Log.Importance>(x.Trim(), true))
            .ToList();

        await RuntimeLogConfig.SaveAllAsync();

        await RespondAsync("Saved new channel logging filters", ephemeral: true);
    }

    public class ChangeLogChannelModal : IModal
    {
        public string Title { get; } = "Change Server Logs Channel";
        
        [InputLabel("Enter the ID of the new log channel")]
        [ModalTextInput("new-channel-id")]
        public string NewChannelId { get; set; }
    }

    public class ModifyLoggingFiltersModal : IModal
    {
        public string Title { get; } = "Edit Logging Filter Settings";
        
        [InputLabel("Enter Filtered Tags")]
        [ModalTextInput("new-tags")]
        public string NewTags { get; set; }
        
        [InputLabel("Enter Filtered Importance Values")]
        [ModalTextInput("new-importances")]
        public string NewImportances { get; set; }
        
        [InputLabel("Enter Filtered Events By Name")]
        [ModalTextInput("new-names")]
        public string NewNames { get; set; }
    }
}
