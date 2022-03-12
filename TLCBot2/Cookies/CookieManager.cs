using System.Text.RegularExpressions;
using TLCBot2.Core;

namespace TLCBot2.Cookies;

public static class CookieManager
{
    public static string CookieDatabasePath => $"{Program.FileAssetsPath}\\database.cookie";

    public static void Initialize()
    {
        if (!File.Exists(CookieDatabasePath))
            File.WriteAllText(CookieDatabasePath, "");
    }

    public static void TakeOrGiveCookiesToUser(ulong userId, int cookieCount = 5)
    {
        if (GetUserFromDatabase(userId, out var cookies, out _))
            AddOrEditUserToDatabase(userId, cookies + cookieCount);
        else 
            AddOrEditUserToDatabase(userId, cookieCount);
    }
    public static void AddOrEditUserToDatabase(ulong userId, int? cookieCount = null, bool? isBanned = null)
    {
        if (GetUserDataFromId(userId, out var formattedData))
        {
            var (_, oldCookies, oldBanStatus) = DeformatUserData(formattedData);
            File.WriteAllLines(CookieDatabasePath,
                File.ReadAllText(CookieDatabasePath).
                    Replace(formattedData, FormatUserData(userId, cookieCount ?? oldCookies, isBanned ?? oldBanStatus))
                    .Split('\n')
                    .Where(x => !string.IsNullOrEmpty(x)));
        }
        else
        {
            File.WriteAllLines(CookieDatabasePath, File.ReadAllLines(CookieDatabasePath)
                .Append(FormatUserData(userId, cookieCount ?? default, isBanned ?? default))
                .Where(x => !string.IsNullOrEmpty(x)));
        }
    }
    public static bool RemoveUserFromDatabase(ulong userId)
    {
        if (!GetUserDataFromId(userId, out var formattedData)) return false;
        File.WriteAllLines(CookieDatabasePath, File.ReadAllLines(CookieDatabasePath)
            .Where(x => x != formattedData && !string.IsNullOrEmpty(x)));
        return true;
    }
    public static bool GetUserFromDatabase(ulong userId, out int cookies, out bool isBanned)
    {
        (cookies, isBanned) = (default, default);

        if (!GetUserDataFromId(userId, out var userLine)) return false;
        (_, cookies, isBanned) = DeformatUserData(userLine);
        return true;
    }
    public static void ClearDatabase()
    {
        File.WriteAllText(CookieDatabasePath, "");
    }
    public static void ResetDatabase(Func<int, int>? defaultCookies = null, Func<bool, bool>? defaultIsBanned = null)
    {
        defaultCookies ??= _ => 0;
        defaultIsBanned ??= _ => false;
        
        File.WriteAllLines(CookieDatabasePath, File.ReadAllLines(CookieDatabasePath).Select(s =>
        {
            var (userId, cookies, isBanned) = DeformatUserData(s);
            return FormatUserData(userId, defaultCookies(cookies), defaultIsBanned(isBanned));
        }).Where(x => !string.IsNullOrEmpty(x)).ToArray());
    }
    public static string FormatUserData(ulong userId, int cookies, bool isBanned)
    {
        return $"USER_ID:{userId}{{COOKIES:{cookies},IS_BANNED:{isBanned}}}";
    }
    public static (ulong userId, int cookies, bool isBanned) DeformatUserData(string formattedData)
    {
        if (string.IsNullOrEmpty(formattedData)) return default;
        
        var userId = ulong.Parse(Regex.Match(formattedData, @"(?<=USER_ID:)\d+(?={)").Value);
        var cookies = int.Parse(Regex.Match(formattedData, $"(?<=USER_ID:{userId}{{.*)\\d+").Value);
        var isBanned = bool.Parse(Regex.Match(formattedData, $"(?<=USER_ID:{userId}{{.*)(True|False)").Value);

        return (userId, cookies, isBanned);
    }

    public static bool GetUserDataFromId(ulong userId, out string formattedData)
    {
        formattedData = "";
        
        var lines = File.ReadAllLines(CookieDatabasePath);
        bool Condition(string x) => Regex.IsMatch(x, $"(?<=USER_ID:{userId}{{).+(?=}})");

        if (lines.Length == 0 || !lines.Any(Condition)) return false;
        
        formattedData = lines.First(Condition);
        return true;
    }
}
