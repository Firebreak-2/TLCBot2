namespace TLCBot2.DataManagement.Temporary;

public record Poll(string Title, Poll.Option[] Options)
{
    public string Title = Title;
    public Option[] Options = Options;
    public List<ulong> VoteHistory = new();

    public record Option(string Title)
    {
        public string Title = Title;
        public int Votes = 0;
    }
}
