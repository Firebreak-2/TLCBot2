namespace TLCBot2.Types;

public record PollData(string Title, PollData.Option[] Options)
{
    public List<ulong> VoterIds { get; set; } = new();

    public record Option(string Value)
    {
        public int Votes { get; set; } = 0;
    }
}