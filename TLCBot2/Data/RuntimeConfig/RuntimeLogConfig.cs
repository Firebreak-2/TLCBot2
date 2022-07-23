using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Logging;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Data.RuntimeConfig;

public static class RuntimeLogConfig
{
    public static readonly string LogDir = $"{Program.FileAssetsPath}/logConfig";

    public static List<Log.Importance> IgnoredLogImportances = new();
    public static List<string> IgnoredLogTags = new();
    public static List<string> IgnoredLogEvents = new();

    public static bool ShouldLog(LogEntry entry)
    {
        if (IgnoredLogImportances.Contains((Log.Importance) entry.Importance))
            return false;
        
        if (IgnoredLogTags.Any(x => entry.Tags.FromJson<string[]>()!.Any(x.CaselessEquals)))
            return false;
        
        if (IgnoredLogEvents.Any(x => x.CaselessEquals(entry.EventName)))
            return false;
        
        return true;
    }

    public static async Task SaveAllAsync()
    {
        await SaveCollectionToAsync(IgnoredLogImportances, $"{LogDir}/{nameof(IgnoredLogImportances)}.json");
        await SaveCollectionToAsync(IgnoredLogTags, $"{LogDir}/{nameof(IgnoredLogTags)}.json");
        await SaveCollectionToAsync(IgnoredLogEvents, $"{LogDir}/{nameof(IgnoredLogEvents)}.json");
    }

    [PreInitialize]
    public static async Task LoadAllAsync()
    {
        IgnoredLogImportances = (await LoadCollectionFromAsync<Log.Importance>($"{LogDir}/{nameof(IgnoredLogImportances)}.json"))!;
        IgnoredLogTags = (await LoadCollectionFromAsync<string>($"{LogDir}/{nameof(IgnoredLogTags)}.json"))!;
        IgnoredLogEvents = (await LoadCollectionFromAsync<string>($"{LogDir}/{nameof(IgnoredLogEvents)}.json"))!;
    }

    public static async Task ModifyAndSaveAsync<T>(string listId, List<T> list, Func<List<T>, Task> modification)
    {
        await modification(list);
        await SaveCollectionToAsync(list, $"{LogDir}/{listId}.json");
    }

    public static async Task SaveCollectionToAsync<TItem>(IEnumerable<TItem> collection, string path)
    {
        await Task.Run(() => File.WriteAllText(path, collection.ToList().ToJson()));
    }

    public static async Task<List<TItem>?> LoadCollectionFromAsync<TItem>(string path)
    {
        string stringData = await Task.Run(() =>
        {
            if (File.Exists(path))
                return File.ReadAllText(path);

            if (!Directory.Exists(LogDir))
                Directory.CreateDirectory(LogDir);
                
            File.WriteAllText(path, "[]");
            return "[]";
        });
        return await Task.FromResult(stringData.FromJson<List<TItem>>());
    }
}