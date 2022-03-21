namespace TLCBot2.DataManagement.Temporary;

public record Poll(string Title, string CustomID, Poll.Option[] Options)
{
    public readonly string CustomID = CustomID;
    public string Title = Title;
    public Option[] Options = Options;
    public List<string> VoteHistory = new();

    public record Option(string Title)
    {
        public string Title = Title;
        public int Votes = 0;
    }
}
