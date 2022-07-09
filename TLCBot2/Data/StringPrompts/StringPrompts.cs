using System.Reflection;
using System.Text.RegularExpressions;
using TLCBot2.Attributes;
using TLCBot2.CommandLine.Commands;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Data.StringPrompts;

public static partial class StringPrompts
{
    public static string DirPath => $"{Program.FileAssetsPath}/StringPrompts";
    public static readonly (FieldInfo Member, IEnumerable<StringPromptFieldAttribute> Attributes)[] Prompts = 
        Helper.GetAllMembersWithAttribute<FieldInfo, StringPromptFieldAttribute>()
            .Where(x => x.Member.FieldType == typeof(string)
                        && x.Member.IsStatic)
            .ToArray();

    [PreInitialize]
    public static async Task Initialize()
    {
        if (!Directory.Exists(DirPath))
        {
            Directory.CreateDirectory(DirPath);
        }

        await Load();
        await EnsureRelevance();
    }

    public static async Task Load()
    {
        foreach ((FieldInfo field, var _) in Prompts)
        {
            const string noValue = "No value provided";
            string filePath = $"{DirPath}/{field.Name}.quack";
            
            if (!File.Exists(filePath))
            {
                field.SetValue(null, noValue);
                await Task.Run(() => File.WriteAllText(filePath, noValue));
                continue;
            }
            
            field.SetValue(null, await Task.Run(() => File.ReadAllText(filePath)));
        }
    }

    public static async Task Save()
    {
        foreach ((FieldInfo field, var _) in Prompts)
        {
            string filePath = $"{DirPath}/{field.Name}.quack";
            await Task.Run(() => File.WriteAllText(filePath, (string) field.GetValue(null)!));
        }
    }

    public static Task EnsureRelevance()
    {
        foreach (var file in Directory.GetFiles(DirPath))
        {
            if (Prompts.Any(x =>
                {
                    string fileName = Regex.Match(file, @"\w+(?=\.\w+$)").Value;
                    return x.Member.Name == fileName;
                }))
                continue;
            
            File.Delete(file);
        }

        return Task.CompletedTask;
    }
}