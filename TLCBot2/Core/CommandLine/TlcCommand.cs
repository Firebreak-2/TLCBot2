namespace TLC_Beta.Core.CommandLine;

public class TlcCommand
{
    public string Name;
    public int Args;
    public Action<string[]> Invoke;
    public string Description;
    public TlcCommand(string name, Action<string[]> invoke, int args = 0, string description = "null")
    {
        Name = name;
        Args = args;
        Invoke = invoke;
        Description = description;
    }
}