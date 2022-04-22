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
            foreach (var reminder in _items)
            {
                var channel = Helper.GetChannelFromId(reminder.ChannelID);
                
                var messages = channel.GetLatestMessages();
                if (!messages.Any()) continue;

                var message = messages.First();

                bool isPreviousMessagePostedByBot = message.Author.IsBot;
                if (isPreviousMessagePostedByBot) continue;

                if (DateTime.Now.Ticks > message.Timestamp.Add(reminder.Delay).Ticks)
                {
                    if (reminder.LatestReminderMessage != null
                        && channel.GetMessageAsync(reminder.LatestReminderMessage.Value).Result is { } prevMessage)
                        prevMessage.DeleteAsync();

                    reminder.LatestReminderMessage =
                        channel.SendMessageAsync(reminder.Message, allowedMentions: AllowedMentions.None).Result.Id;
                }
            }
        };
    }
    public record BotReminder(string ReminderID, ulong ChannelID, TimeSpan Delay, string Message)
    {
        public ulong? LatestReminderMessage;
        public string Format() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static BotReminder? Deformat(string data) => JsonConvert.DeserializeObject<BotReminder>(data);
    }
    private static List<BotReminder> _items = new();
    public static string DatabasePath => $"{Program.FileAssetsPath}/botReminders.json";
    public static void SaveAll() =>
        File.WriteAllText(DatabasePath, JsonConvert.SerializeObject(_items, Formatting.Indented));
    public static bool Load(Func<BotReminder, bool> condition, out BotReminder? reminder) => 
        LoadAll().TryFirst(condition, out reminder);
    public static IEnumerable<BotReminder> LoadAll()
    {
        if (File.Exists(DatabasePath))
            _items = JsonConvert.DeserializeObject<List<BotReminder>>(File.ReadAllText(DatabasePath))!;
        else
            SaveAll();
        return _items;
    }
    public static void Add(BotReminder reminder) => Modify(list => { list.Add(reminder); return list; });
    public static void Add(params BotReminder[] reminder) => Modify(list => { list.AddRange(reminder); return list; });
    public static void Remove(BotReminder reminder) =>  Modify(list => { list.Remove(reminder); return list; });
    public static void RemoveAll(Predicate<BotReminder> condition) => Modify(list => { list.RemoveAll(condition); return list; });
    public static void Modify(Func<List<BotReminder>, IEnumerable<BotReminder>> action)
    {
        _items = action(LoadAll().ToList()).ToList();
        SaveAll();
    }
}