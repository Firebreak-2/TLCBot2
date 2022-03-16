namespace TLCBot2.DataManagement.Temporary;

public static class PollHandler
{
    public static List<Poll> Polls = new ();
    public record Poll(string Title, string CustomID, PollOption[] Options)
    {
        public readonly string CustomID = CustomID;
        public string Title = Title;
        public PollOption[] Options = Options;
    }
    public record PollOption(string Title, string Description, int Votes, string CustomID)
    {
        public readonly string CustomID = CustomID;
        public string Title = Title;
        public string Description = Description;
        public int Votes = Votes;
    }
    public static void Add(Poll poll) => Polls.Add(poll);

    public static void Vote(string pollCustomID, string pollOptionCustomID)
    {
        Polls.First(x =>
            x.CustomID == pollCustomID)
            .Options.First(x =>
                x.CustomID == pollOptionCustomID).Votes++;
    }

    public static bool GetPoll(string pollCustomID, out Poll poll)
    {
        poll = default!;
        
        bool Condition(Poll x) => x.CustomID == pollCustomID;
        if (!Polls.Any(Condition)) return false;

        poll = Polls.First(Condition);
        return true;
    }
}