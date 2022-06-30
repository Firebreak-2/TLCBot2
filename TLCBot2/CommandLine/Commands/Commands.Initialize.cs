using System.Reflection;
using Discord;
using TLCBot2.Attributes;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    public static IUser? LastCommandUser = null; 
    
    public static readonly List<(MethodInfo Method, TerminalCommandAttribute Attribute)> All = new();

    static TerminalCommands()
    {
        All.AddRange(Helper.GetAllMembersWithAttribute<MethodInfo, TerminalCommandAttribute>()
            .Where(x => x.Member.ReturnType.IsAssignableTo(typeof(Task)))
            .Select(x => (x.Member, x.Attributes.First())));
    }
}