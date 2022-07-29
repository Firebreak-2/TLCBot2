using TLCBot2.ApplicationCommands;
using TLCBot2.Attributes;
using TLCBot2.Data;
using TLCBot2.Logging;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.Listeners;

public static partial class Listener
{
    [PreInitialize]
    public static async Task OnEventLogged()
    {
        Log.OnEventLog += async (eventName, entry) =>
        {
            await using var db = new TlcDbContext();
            foreach (var eventLogAction in db.EventLogActions)
            {
                if (!InteractionCommands.ServerLogsGroup.CheckIfLogEntryPassesFilters(
                        entry,
                        eventLogAction.OnTags?.FromJson<List<List<string>>>(),
                        eventLogAction.OnEvents?.FromJson<List<string>>(),
                        eventLogAction.OnImportances?.FromJson<List<Log.Importance>>()?
                            .Select(x => $"=;{x.ToString()}")
                            .ToList(),
                        null)) continue;
                
                var msg = await Helper.MessageFromJumpUrl(eventLogAction.ActionLink);
                if (Helper.ExtractCodeFromCodeBlock(msg.Content) is var (_, code)
                    && code.FromJson<MessageComponentAction>() is { } action)
                {
                    action.Execute<object>(entry.ToUsable());
                }
            }
        };
    }
}
