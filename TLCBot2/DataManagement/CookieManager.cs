using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TLCBot2.Core;

namespace TLCBot2.DataManagement;

public static class CookieManager
{
    public static string CookieDatabasePath => $"{Program.FileAssetsPath}\\cookieDatabase.txt";
    public static List<CookieUserEntry> CookieUsers = new();
    public static void Initialize()
    {
        if (!File.Exists(CookieDatabasePath))
        {
            SaveCookieUsersToDatabase();
            return;
        }

        CookieUsers = JsonConvert.DeserializeObject<List<CookieUserEntry>>(File.ReadAllText(CookieDatabasePath))!;
    }

    public static void SaveCookieUsersToDatabase()
    {
        File.WriteAllText(CookieDatabasePath, JsonConvert.SerializeObject(CookieUsers, Formatting.Indented));
    }
    public static bool ModifyUser(ulong userID, 
        int? cookies = null,
        bool? isBanned = null)
    {
        bool Condition(CookieUserEntry x) => x.UserID == userID;
        if (!CookieUsers.Any(Condition)) return false;
        var user = CookieUsers.First(Condition);
        
        if (cookies .HasValue) user.Cookies = cookies.Value;
        if (isBanned.HasValue) user.IsBanned = isBanned.Value;
        SaveCookieUsersToDatabase();
        return true;
    }
    public static CookieUserEntry AddOrModifyUser(ulong userID, 
        int? cookies = null,
        bool? isBanned = null)
    {
        if (ModifyUser(userID, cookies, isBanned))
            return CookieUsers.First(x => x.UserID == userID);

        var newUser = new CookieUserEntry(userID, cookies ?? default, isBanned ?? default);
        CookieUsers.Add(newUser);
        SaveCookieUsersToDatabase();
        return newUser;
    }
    public static bool GetUser(ulong userID, out CookieUserEntry userEntry)
    {
        userEntry = null!;
        
        bool Condition(CookieUserEntry x) => x.UserID == userID;
        if (!CookieUsers.Any(Condition)) return false;
        userEntry = CookieUsers.First(Condition);
        return true;
    }
    public static void TakeOrGiveCookiesToUser(ulong userId, int cookieCount = 5)
    {
        if (GetUser(userId, out var user))
            ModifyUser(userId, user.Cookies + cookieCount, user.IsBanned);
        else
            AddOrModifyUser(userId, cookieCount, false);
    }
    public static void ClearDatabase()
    {
        CookieUsers.Clear();
        SaveCookieUsersToDatabase();
    }
    public static void ResetDatabase(Func<int, int>? defaultCookies = null, Func<bool, bool>? defaultIsBanned = null)
    {
        defaultCookies ??= _ => 0;
        defaultIsBanned ??= _ => false;

        CookieUsers = CookieUsers
            .Select(x =>
                new CookieUserEntry(
                    x.UserID,
                    defaultCookies(x.Cookies),
                    defaultIsBanned(x.IsBanned)))
            .ToList();
        SaveCookieUsersToDatabase();
    }
    public record CookieUserEntry(ulong UserID, int Cookies, bool IsBanned)
    {
        public ulong UserID = UserID;
        public int Cookies = Cookies;
        public bool IsBanned = IsBanned;
    }
}
