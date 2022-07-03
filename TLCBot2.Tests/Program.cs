using Newtonsoft.Json;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.tests;

public class Program
{
    public static void Main(string[] args)
    {
        var data = new PollData("Poll Title", new[]
        {
            new PollData.Option("Yes"),
            new PollData.Option("No"),
        });
        
        Console.WriteLine(data.ToJson());
    }
}