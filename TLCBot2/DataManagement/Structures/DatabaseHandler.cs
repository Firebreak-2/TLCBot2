namespace TLCBot2.DataManagement.Structures;

public static class DatabaseHandler
{
    public static List<IDatabase<IDataItem>> AllDatabases = new();

    public static void FireStarter()
    {
        
    }
    public static void Initialize()
    {
        AllDatabases.ForEach(x => x.Initialize());
    }
}