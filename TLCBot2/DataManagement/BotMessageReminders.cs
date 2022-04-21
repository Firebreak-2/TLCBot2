using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using TLCBot2.Core;
using TLCBot2.Listeners.TimedEvents;
using TLCBot2.Utilities;

namespace TLCBot2.DataManagement;

public static class BotMessageReminders
{
    public static void Initialize()
    {
        LoadAll();
        TheFlowOfTime.MinuteCLock.Elapsed += (_, _) =>
        {
            foreach (var reminder in Items)
            {
                var channel = Helper.GetChannelFromId(reminder.ChannelID);

                var messages = channel.GetLatestMessages();
                
                var message = messages.First();

                bool isPreviousMessagePostedByBot = message.Author.IsBot;
                if (isPreviousMessagePostedByBot) continue;
                
                if (DateTime.Now.Ticks > message.Timestamp.Add(reminder.Delay).Ticks)
                    channel.SendMessageAsync(reminder.Message, allowedMentions: AllowedMentions.None);
            }
        };
    }

    public static Task OnMessageReceived(SocketMessage message)
    {
        var latestMessages = ((SocketTextChannel) message.Channel).GetLatestMessages(10);

        foreach (var msg in latestMessages)
        {
            if (msg.Author.Id != Program.Client.CurrentUser.Id || Items.All(x => x.Message != msg.Content)) continue;
            msg.DeleteAsync();
            break;
        }
        
        return Task.CompletedTask;
    }
    public record BotReminder(string ReminderID, ulong ChannelID, TimeSpan Delay, string Message)
    {
        public string Format() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static BotReminder? Deformat(string data) => JsonConvert.DeserializeObject<BotReminder>(data);
    }
    public static List<BotReminder> Items = new();
    public static string DatabasePath => $"{Program.FileAssetsPath}/botReminders.json";
    public static void SaveAll() =>
        File.WriteAllText(DatabasePath, JsonConvert.SerializeObject(Items));
    public static bool Load(Func<BotReminder, bool> condition, out BotReminder? reminder) => 
        LoadAll().TryFirst(condition, out reminder);
    public static IEnumerable<BotReminder> LoadAll()
    {
        if (File.Exists(DatabasePath))
            Items = JsonConvert.DeserializeObject<List<BotReminder>>(File.ReadAllText(DatabasePath))!;
        else
            SaveAll();
        return Items;
    }
    public static void Add(BotReminder reminder) => Modify(list => { list.Add(reminder); return list; });
    public static void Add(params BotReminder[] reminder) => Modify(list => { list.AddRange(reminder); return list; });
    public static void Remove(BotReminder reminder) =>  Modify(list => { list.Remove(reminder); return list; });
    public static void RemoveAll(Predicate<BotReminder> condition) => Modify(list => { list.RemoveAll(condition); return list; });
    public static void Modify(Func<List<BotReminder>, IEnumerable<BotReminder>> action)
    {
        Items = action(LoadAll().ToList()).ToList();
        SaveAll();
    }
}