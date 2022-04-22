using Newtonsoft.Json;

namespace TLCBot2.DataManagement.Structures;

public interface IDataItem
{
    public string Format();
    public static IDataItem? Deformat(string data) => JsonConvert.DeserializeObject<IDataItem>(data);
}