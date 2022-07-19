using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TLCBot2.Data;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Logging;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [SlashCommand("give-cookies", "Give or Take cookies from a user with the summoned modal")]
    [UserCommand("Give Cookies")]
    public async Task GiveCookies(SocketGuildUser user)
    {
        await RespondWithModalAsync<GiveCookiesModal>($"give-cookies-modal;{user.Id}");
    }
    
    public class GiveCookiesModal : IModal
    {
        public string Title { get; } = "Give Cookies";
        
        [InputLabel("The amount of cookies to give or take")]
        [ModalTextInput("0", initValue: "5")]
        public string Amount { get; set; }

        [InputLabel("The reason for giving this user the cookies")]
        [ModalTextInput("1", TextInputStyle.Paragraph, "They gave good critisicm to X when they asked for help about Y")]
        [RequiredInput(false)]
        public string Reason { get; set; } = "No reason provided";
    }
    
    [ModalInteraction("give-cookies-modal;*")]
    public async Task GiveCookiesModalResponse(string id, GiveCookiesModal modal)
    {
        var user = RuntimeConfig.FocusServer!.GetUser(id.To<ulong>());
        await using var db = new TlcDbContext();
        
        int oldBalance = 0;
        int newBalance = modal.Amount.To<int>();
        
        // checks if the user exists in the db, if yes
        // then changes the balances to match that. otherwise,
        // adds a new entry with the current balances
        if (await db.Users.FindAsync(user.Id) is { } userEntry)
        {
            oldBalance = userEntry.Balance;
            newBalance += oldBalance;
            userEntry.Balance = newBalance;
        }
        else
        {
            await db.Users.AddAsync(new ProfileEntry(user.Id)
            {
                Balance = newBalance
            });
        }

        await db.SaveChangesAsync();

        await Log.CookieTransaction(Context.User, user, oldBalance, newBalance, 
            modal.Reason is null or { Length: 0 } ? null : modal.Reason);

        await RespondAsync($"{user.Mention}'s cookies have been updated. ({oldBalance} -> {newBalance})",
            ephemeral: true, allowedMentions: AllowedMentions.None);
    }
}