using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.CommandLine;
using TLCBot2.CommandLine.Commands;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Logging;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [ComponentInteraction("ea-button;*,*")]
    public async Task ManageEventActionsButton(string action, string modifyId)
    {
        switch (action)
        {
            case "ls": // list current event actions
            {
                await using var db = new TlcDbContext();
                var entries = db.EventLogActions.Take(125).ToArray();

                if (!entries.Any())
                {
                    await RespondAsync("There are no entries in the database", ephemeral: true);
                    break;
                }
                //          smId   elements
                Dictionary<string, string?[]> menus = new();

                for (int i = 0; i < entries.Length; i++)
                {
                    int iElement = Math.DivRem(i, 25, out int iMenu);

                    string smId = $"ea-ls-selectmenu;{iMenu}";
                    
                    if (iElement == 0)
                        menus.Add(smId, new string[25]);
                    
                    menus[smId][iElement] = entries[i].ID;
                }

                await RespondAsync(
                    components: new ComponentBuilder()
                        .WithRows(menus.Select(x => new ActionRowBuilder()
                            .WithSelectMenu(x.Key, x.Value
                                .Where(y => y is not null)
                                .Select(y => new SelectMenuOptionBuilder()
                                    .WithLabel(y)
                                    .WithValue(y))
                                .ToList())
                        ))
                        .Build(),
                    ephemeral: true);
                break;
            }
            case "add": // add a new event action
            {
                await RespondWithModalAsync<AddEventActionModal>("ea-am-modal;0");
                break;
            }
            case "mod": // modify an existing event action with id
            {
                await using var db = new TlcDbContext();
                if (await db.EventLogActions.FindAsync(modifyId) is not { } entry)
                    throw new Exception($"Event action entry with ID [{modifyId}] does not exist");
                
                await Context.Interaction.RespondWithModalAsync<AddEventActionModal>($"ea-am-modal;{modifyId}",
                    modifyModal: modal =>
                    {
                        modal.Components = new ModalComponentBuilder()
                            .WithTextInput(new TextInputBuilder()
                                .WithRequired(false)
                                .WithLabel("On Tags")
                                .WithCustomId("tags")
                                .WithValue(string.Join(", ", 
                                    (entry.OnTags?.FromJson<List<List<string>>>() ?? new List<List<string>>())
                                        .Select(x => string.Join('+', x))))
                            )
                            .WithTextInput(new TextInputBuilder()
                                .WithRequired(false)
                                .WithLabel("On Importance Values")
                                .WithCustomId("importances")
                                .WithValue(string.Join(", ",
                                    (entry.OnImportances?.FromJson<List<Log.Importance>>() ?? new List<Log.Importance>())
                                        .Select(x => x.ToString())))
                            )
                            .WithTextInput(new TextInputBuilder()
                                .WithRequired(false)
                                .WithLabel("On Events By Name")
                                .WithCustomId("names")
                                .WithValue(string.Join(", ",
                                    entry.OnEvents?.FromJson<List<string>>() ?? new List<string>()))
                            )
                            .WithTextInput(new TextInputBuilder()
                                .WithRequired(false)
                                .WithLabel("Jump URL To Action Message")
                                .WithCustomId("action")
                                .WithValue(entry.ActionLink)
                            );
                    });
                break;
            }
            default:
                throw new NotImplementedException();
        }
    }

    [ModalInteraction("ea-am-modal;*")]
    public async Task AddEventActionModalResponse(string modify, AddEventActionModal modal)
    {
        await using var db = new TlcDbContext();

        // im converting them to arrays instead
        // of lists because i think that would be
        // faster and json sees lists and arrays as
        // the same thing
        var onEvents = modal.Names?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray()
            .ToJson();
        var onImportances = modal.Importances?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Enum.Parse<Log.Importance>(x.Trim(), true))
            .ToArray()
            .ToJson();
        var onTags = modal.Tags?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().Split('+'))
            .ToArray()
            .ToJson();
        
        if (modify == "0") // add
        {
            if (db.EventLogActions.Count() > 125)
                throw new Exception("Entry limit reached [125]");

            if (await db.EventLogActions.FindAsync(modal.Id) is not null)
            {
                await RespondAsync($"Event action with ID [{modal.Id}] already exists", ephemeral: true);
                return;
            }
            
            await db.EventLogActions.AddAsync(new EventLogActionEntry(modal.Id)
            {
                OnEvents = onEvents,
                OnImportances = onImportances,
                OnTags = onTags,
                ActionLink = modal.ActionLink
            });

            await RespondAsync($"Added event action [{modal.Id}]", ephemeral: true);
        }
        else // modify
        {
            if (await db.EventLogActions.FindAsync(modify) is not { } entry)
                throw new Exception($"Event action entry with ID [{modify}] does not exist");

            if (modal is {ActionLink: not null})
                entry.ActionLink = modal.ActionLink;
            
            entry.OnEvents = onEvents;
            entry.OnImportances = onImportances;
            entry.OnTags = onTags;

            await RespondAsync($"Modified event action [{modal.Id}]", ephemeral: true);
        }
        
        await db.SaveChangesAsync();
    }

    public class AddEventActionModal : IModal
    {
        public string Title { get; } = "Enter new event action values";
        
        [InputLabel("Event Action ID")]
        [ModalTextInput("id")]
        public string Id { get; set; }
        
        [InputLabel("On Tags")]
        [ModalTextInput("tags")]
        [RequiredInput(false)]
        public string? Tags { get; set; }
        
        [RequiredInput(false)]
        [InputLabel("On Importance Values")]
        [ModalTextInput("importances")]
        public string? Importances { get; set; }
        
        [RequiredInput(false)]
        [InputLabel("On Events By Name")]
        [ModalTextInput("names")]
        public string? Names { get; set; }
        
        [InputLabel("Jump URL To Action Message")]
        [ModalTextInput("action")]
        public string ActionLink { get; set; }
    }
}
