using Discord;
using Newtonsoft.Json;
using TLCBot2.Core;

namespace TLCBot2.DataManagement;

public static class ChannelVisiblityData
{
    public static string Path => $"{Program.FileAssetsPath}/channels.json";

    public static void Initialize()
    {
        if (!File.Exists(Path))
            Save(new Dictionary<ulong, Overwrite[]>());
    }

    public static Dictionary<ulong, Overwrite[]>? Load() =>
        JsonConvert.DeserializeObject<Dictionary<ulong, Overwrite[]>>(File.ReadAllText(Path));
    public static void Save(Dictionary<ulong, Overwrite[]> channels) =>
        File.WriteAllText(Path, JsonConvert.SerializeObject(channels, Formatting.Indented));
}