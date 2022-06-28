﻿using System.Reflection;
using TLCBot2.Attributes;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    public static readonly List<(MethodInfo Method, TerminalCommandAttribute Attribute)> All = new();

    [Initialize]
    public static Task Initialize()
    {
        All.AddRange(Helper.GetAllMembersWithAttribute<MethodInfo, TerminalCommandAttribute>()
            .Where(x => x.Member.ReturnType.IsAssignableTo(typeof(Task)))
            .Select(x => (x.Member, x.Attributes.First())));

        return Task.CompletedTask;
    }
}