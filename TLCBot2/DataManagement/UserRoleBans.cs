using Newtonsoft.Json;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.DataManagement;

public static class UserRoleBans
{
    public static void Initialize() => LoadAll();
    public record RoleBan(ulong UserID, ulong[] RolesBannedIDs)
    {
        public string Format() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static RoleBan? Deformat(string data) => JsonConvert.DeserializeObject<RoleBan>(data);
    }
    private static List<RoleBan> _items = new();
    public static string DatabasePath => $"{Program.FileAssetsPath}/userRoleBans.json";
    public static void SaveAll() =>
        File.WriteAllText(DatabasePath, JsonConvert.SerializeObject(_items, Formatting.Indented));
    public static bool Load(Func<RoleBan, bool> condition, out RoleBan? roleBan) => 
        LoadAll().TryFirst(condition, out roleBan);
    public static IEnumerable<RoleBan> LoadAll()
    {
        if (File.Exists(DatabasePath))
            _items = JsonConvert.DeserializeObject<List<RoleBan>>(File.ReadAllText(DatabasePath))!;
        else
            SaveAll();
        return _items;
    }
    public static void Add(RoleBan roleBan) => Modify(list => { list.Add(roleBan); return list; });
    public static void Add(params RoleBan[] roleBan) => Modify(list => { list.AddRange(roleBan); return list; });
    public static void Remove(RoleBan roleBan) =>  Modify(list => { list.Remove(roleBan); return list; });
    public static void RemoveAll(Predicate<RoleBan> condition) => Modify(list => { list.RemoveAll(condition); return list; });
    public static void Modify(Func<List<RoleBan>, IEnumerable<RoleBan>> action)
    {
        _items = action(LoadAll().ToList()).ToList();
        SaveAll();
    }
}