using System.Reflection;
using TLCBot2.Utilities;
using Discord.WebSocket;
using TLCBot2.Core;

namespace TLCBot2.Attributes;


/// <summary>
/// Runs the method using the attribute before Discord's
/// <see cref="DiscordSocketClient.Ready"/> event.
/// Only accepts async methods with no parameters
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PreInitializeAttribute : Attribute
{
    public static readonly (Func<Task> Action, PreInitializeAttribute Attribute)[] MethodsUsing;

    static PreInitializeAttribute()
    {
        // gets all the methods using this attribute which
        // have no parameters
        var methods = Helper.GetAllMembersWithAttribute<MethodInfo, PreInitializeAttribute>()
            .Where(x => !x.Member.GetParameters().Any()
                && x.Member.ReturnType.IsAssignableTo(typeof(Task))
                && x.Member.IsStatic)
            .ToArray();

        int length = methods.Length;
        MethodsUsing = new (Func<Task>, PreInitializeAttribute)[length];

        for (int i = 0; i < length; i++)
        {
            var (method, attr) = methods[i];
            MethodsUsing[i] = (() => (Task) method.Invoke(null, null)!, attr.First());
        }
    }

    public int Priority { get; set; } = 0;
}