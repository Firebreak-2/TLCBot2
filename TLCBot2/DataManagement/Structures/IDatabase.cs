namespace TLCBot2.DataManagement.Structures;

public interface IDatabase<TDataItem> where TDataItem : IDataItem
{
    public void Initialize();
    public IEnumerable<TDataItem> Items { get; set; }
    public string DatabasePath { get; }
    public void SaveAll();
    public IEnumerable<TDataItem> LoadAll();
    public void Add(TDataItem reminder) => Modify(list => { list.Add(reminder); return list; });
    public void Add(params TDataItem[] reminder) => Modify(list => { list.AddRange(reminder); return list; });
    public void Remove(TDataItem reminder) =>  Modify(list => { list.Remove(reminder); return list; });
    public void RemoveAll(Predicate<TDataItem> condition) => Modify(list => { list.RemoveAll(condition); return list; });
    public void Modify(Func<List<TDataItem>, IEnumerable<TDataItem>> action)
    {
        Items = action(LoadAll().ToList()).ToList();
        SaveAll();
    }
}