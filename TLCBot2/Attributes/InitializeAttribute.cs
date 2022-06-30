using System.Reflection;
using TLCBot2.Utilities;
using Discord.WebSocket;

namespace TLCBot2.Attributes;

/// <summary>
/// Runs the method when Discord's <see cref="DiscordSocketClient.Ready"/> event fires
/// Only accepts async methods with no parameters
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class InitializeAttribute : Attribute
{
    public static readonly (Func<Task> Action, InitializeAttribute Attribute)[] MethodsUsing;

    static InitializeAttribute()
    {
        // gets all the methods using this attribute which
        // have no parameters
        var methods = Helper.GetAllMembersWithAttribute<MethodInfo, InitializeAttribute>()
            .Where(x => !x.Member.GetParameters().Any()
                        && x.Member.ReturnType.IsAssignableTo(typeof(Task))
                        && x.Member.IsStatic)
            .ToArray();

        int length = methods.Length;
        MethodsUsing = new (Func<Task>, InitializeAttribute)[length];

        for (int i = 0; i < length; i++)
        {
            var method = methods[i].Member;
            MethodsUsing[i] = (() => (Task) method.Invoke(null, null)!, methods[i].Attributes.First());
        }
    }

    public int Priority { get; set; } = 0;
}