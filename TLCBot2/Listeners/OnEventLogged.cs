using TLCBot2.Attributes;
using TLCBot2.Logging;

namespace TLCBot2.Listeners;

public static partial class Listener
{
    [PreInitialize]
    public static async Task OnEventLogged()
    {
        Log.OnEventLog += async (eventName, entry) =>
        {
            
        };
    }
}
