using Discord;

namespace TLCBot2.DataManagement.Cookies;

public static class CookieShop
{
    public record ItemData(string Name, string Description, int? BuyLimit, int Price);
    public static readonly (ItemData ItemData, Action<IUser> onPurchase)[] Contents = {
        (new ItemData("Test Item", "An item designed for testing purposes", null, 5), 
            customer => customer.SendMessageAsync("hello there")),
        (new ItemData("Expensive Stuff", "More expensive than your mother", null, 999), 
            customer => customer.SendMessageAsync("expensivado")),
    };
    public static EmbedBuilder GenerateShopEmbed()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Cookie Shop")
            .WithColor(Color.Blue);
        int i = 0;
        foreach ((ItemData itemData, _) in Contents)
        {
            if (i++ >= EmbedBuilder.MaxFieldCount) break;
            
            embed.AddField($"{itemData.Price} 🍪 | {itemData.Name}", itemData.Description);
        }
        return embed;
    }
    public static void BuyItem(IUser buyer, string itemName) =>
        Contents.First(x => 
            x.ItemData.Name == itemName).onPurchase(buyer);
}