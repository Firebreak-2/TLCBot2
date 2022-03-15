using Newtonsoft.Json;
using TLCBot2.Core;
#pragma warning disable CS0414

namespace TLCBot2.DataManagement;

public static class SocialMediaManager
{
    public static string DatabasePath => $"{Program.FileAssetsPath}\\smDatabase.txt";
    public static List<SocialMediaUserEntry> SMUsers = new();
    public static void Initialize()
    {
        if (!File.Exists(DatabasePath))
        {
            SaveSMUsersToDatabase();
            return;
        }

        SMUsers = JsonConvert.DeserializeObject<List<SocialMediaUserEntry>>(File.ReadAllText(DatabasePath))!;
    }

    public static void SaveSMUsersToDatabase()
    {
        File.WriteAllText(DatabasePath, JsonConvert.SerializeObject(SMUsers, Formatting.Indented));
    }
    public static bool ModifyUser(ulong userID, 
        string? Instagram = null,
        string? Twitter = null,
        string? Youtube = null,
        string? ArtStation = null,
        string? Reddit = null,
        string? GitHub = null,
        string? Steam = null,
        string? DeviantArt = null,
        string? TikTok = null,
        string? Twitch = null,
        string? PersonalWebsite = null)
    {
        bool Condition(SocialMediaUserEntry x) => x.UserID == userID;
        if (!SMUsers.Any(Condition)) return false;
        var user = SMUsers.First(Condition);
        
        if (Instagram is not null      ) user.Instagram = Instagram;
        if (Twitter is not null        ) user.Twitter = Twitter;
        if (Youtube is not null        ) user.Youtube = Youtube;
        if (ArtStation is not null     ) user.ArtStation = ArtStation;
        if (DeviantArt is not null     ) user.DeviantArt = DeviantArt;
        if (TikTok is not null         ) user.TikTok = TikTok;
        if (Twitch is not null         ) user.Twitch = Twitch;
        if (Reddit is not null     ) user.DeviantArt = Reddit;
        if (GitHub is not null         ) user.TikTok = GitHub;
        if (Steam is not null         ) user.Twitch = Steam;
        if (PersonalWebsite is not null) user.PersonalWebsite = PersonalWebsite;
        SaveSMUsersToDatabase();
        return true;
    }
    public static SocialMediaUserEntry AddOrModifyUser(ulong userID, 
        string? Instagram = null,
        string? Twitter = null,
        string? Youtube = null,
        string? ArtStation = null,
        string? DeviantArt = null,
        string? TikTok = null,
        string? Twitch = null,
        string? Reddit = null,
        string? GitHub = null,
        string? Steam = null,
        string? PersonalWebsite = null)
    {
        if (ModifyUser(
                userID,
                Instagram,
                Twitter,
                Youtube,
                ArtStation,
                Reddit,
                GitHub,
                Steam,
                DeviantArt,
                TikTok,
                Twitch,
                PersonalWebsite
                ))
        {
            return SMUsers.First(x => x.UserID == userID);
        }

        var newUser = new SocialMediaUserEntry(userID)
        {
            Instagram =       Instagram       ?? SocialMediaUserEntry.NoLink,
            Twitter =         Twitter         ?? SocialMediaUserEntry.NoLink,
            Youtube =         Youtube         ?? SocialMediaUserEntry.NoLink,
            ArtStation =      ArtStation      ?? SocialMediaUserEntry.NoLink,
            DeviantArt =      DeviantArt      ?? SocialMediaUserEntry.NoLink,
            TikTok =          TikTok          ?? SocialMediaUserEntry.NoLink,
            Twitch =          Twitch          ?? SocialMediaUserEntry.NoLink,
            Reddit =          Reddit          ?? SocialMediaUserEntry.NoLink,
            GitHub =          GitHub          ?? SocialMediaUserEntry.NoLink,
            Steam =           Steam           ?? SocialMediaUserEntry.NoLink,
            PersonalWebsite = PersonalWebsite ?? SocialMediaUserEntry.NoLink
        };
        SMUsers.Add(newUser);
        SaveSMUsersToDatabase();
        return newUser;
    }
    public static bool GetUser(ulong userID, out SocialMediaUserEntry userEntry)
    {
        userEntry = null!;
        
        bool Condition(SocialMediaUserEntry x) => x.UserID == userID;
        if (!SMUsers.Any(Condition)) return false;
        userEntry = SMUsers.First(Condition);
        return true;
    }
    public class SocialMediaUserEntry
    {
        public const string NoLink = "%NULL%";
        public ulong UserID;
        public string Instagram = NoLink;
        public string Twitter = NoLink;
        public string Youtube = NoLink;
        public string ArtStation = NoLink;
        public string DeviantArt = NoLink;
        public string TikTok = NoLink;
        public string Twitch = NoLink;
        public string Reddit = NoLink;
        public string Steam = NoLink;
        public string GitHub = NoLink;
        public string PersonalWebsite = NoLink;
        public SocialMediaUserEntry(ulong userId)
        {
            UserID = userId;
        }
    }
}