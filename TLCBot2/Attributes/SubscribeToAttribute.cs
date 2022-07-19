using System.Reflection;
using Discord.WebSocket;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class SubscribeToAttribute : Attribute
{
    // [PreInitialize]
    public static Task Initialize()
    {
        var methods = Helper.GetAllMembersWithAttribute<MethodInfo, SubscribeToAttribute>();
        foreach ((MethodInfo method, IEnumerable<SubscribeToAttribute> attributes) in methods)
        {
            var attribute = attributes.First();

            var eventInfo = typeof(DiscordSocketClient).GetEvent(attribute.EventName);
            Type typeDelegate = eventInfo!.EventHandlerType!;
            
            Delegate d = Delegate.CreateDelegate(typeDelegate, Program.Client, method);
            
            MethodInfo addMethod = eventInfo.GetAddMethod()!;
            object[] addHandlerArgs = { d };
            addMethod.Invoke(Program.Client, addHandlerArgs);
        }

        return Task.CompletedTask;
    }
    
    public SubscribeToAttribute(string eventName)
    {
        EventName = eventName;
    }
    
    public string EventName { get; }
}