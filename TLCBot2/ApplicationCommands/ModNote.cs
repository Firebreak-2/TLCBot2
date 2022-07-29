using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Data;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [UserCommand("Mod Note")]
    [SlashCommand("mod-note", "Read or write the user's mod note")]
    public async Task ModNote(SocketGuildUser user)
    {
        await using var db = new TlcDbContext();
        
        if (await db.Users.FindAsync(user.Id) is {ModNote: { }} entry)
        {
            await RespondAsync(entry.ModNote.ToCodeBlock(), components: new ComponentBuilder()
                .WithButton("Edit Note", $"edit-note-button;{user.Id},m")
                .Build(), ephemeral: true);
            return;
        }

        await RespondAsync("User does not have a note", components: new ComponentBuilder()
            .WithButton("Add Note", $"edit-note-button;{user.Id},c")
            .Build(), ephemeral: true);
    }

    [ComponentInteraction("edit-note-button;*,*")]
    public async Task EditNoteButtonResponse(string userIdString, string mode)
    {
        ulong messageId = ((SocketMessageComponent) Context.Interaction).Message.Id;
        
        switch (mode)
        {
            case "m": // modify note
            {
                await using var db = new TlcDbContext();
                string modnote = (await db.Users.FindAsync(userIdString.To<ulong>()))!.ModNote!;

                await Context.Interaction.RespondWithModalAsync<EditNoteModal>($"edit-note-modal;{userIdString}",
                    modifyModal: modal =>
                    {
                        modal.Components = new ModalComponentBuilder()
                            .WithTextInput(
                                "The new mod note for the user",
                                "note",
                                TextInputStyle.Paragraph,
                                minLength: 0,
                                maxLength: 4000 - 8,
                                required: false,
                                value: modnote);
                    });
                break;
            }
            case "c": // create note
            {
                await RespondWithModalAsync<EditNoteModal>($"edit-note-modal;{userIdString}");
                break;
            }
        }
    }

    public class EditNoteModal : IModal
    {
        public string Title { get; } = "Create Note";
        
        [InputLabel("The mod note for the user")]
        [ModalTextInput("note", TextInputStyle.Paragraph, minLength: 0, maxLength: 4000 - 8)]
        [RequiredInput(false)]
        public string? Note { get; set; }
    }

    [ModalInteraction("edit-note-modal;*")]
    public async Task EditNoteModalResponse(string userIdString, EditNoteModal modal)
    {
        if (modal.Note?.Length == 0)
            modal.Note = null;
        
        await using var db = new TlcDbContext();
        if (await db.Users.FindAsync(userIdString.To<ulong>()) is { } entry)
            entry.ModNote = modal.Note;
        else
            await db.Users.AddAsync(new ProfileEntry(userIdString.To<ulong>())
            {
                ModNote = modal.Note
            });
        await db.SaveChangesAsync();

        if (modal.Note is not null)
        {
            await ((SocketModal) Context.Interaction).UpdateAsync(props =>
            {
                props.Content = modal.Note.ToCodeBlock();
                props.Components = new ComponentBuilder()
                    .WithButton("Edit Note", $"edit-note-button;{userIdString},m")
                    .Build();
            });
        }
        else
        {
            await ((SocketModal) Context.Interaction).UpdateAsync(props =>
            {
                props.Content = "User does not have a note";
                props.Components = new ComponentBuilder()
                    .WithButton("Add Note", $"edit-note-button;{userIdString},c")
                    .Build();
            });
        }
    }
}