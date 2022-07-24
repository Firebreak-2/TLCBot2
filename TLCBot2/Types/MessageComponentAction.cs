using Newtonsoft.Json.Linq;
using TLCBot2.MessageBuilder.DiscordActions;

namespace TLCBot2.Types;

public record MessageComponentAction
{
    public string? ActionId = null;
    public Dictionary<string, object>? Parameters = null;

    public string Execute(params (string, object)[] externalParameters)
    {
        if (ActionId is null)
            return "Action is null";

        if (!DiscordMethods.AllActions.TryGetValue(ActionId, out var method))
            return $"Action [{ActionId}] does not exist";
        
        var parameters = method.Parameters;
        object?[]? deserializedParameters = null;
        if (Parameters is not null)
        {
            deserializedParameters = new object[parameters!.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                object obj = Parameters[parameters[i].Name];
                deserializedParameters[i] = obj is JObject jobj 
                    ? jobj.ToObject(parameters[i].Type) 
                    : Convert.ChangeType(obj, parameters[i].Type);
            }
        }

        method.Invoke(deserializedParameters);

        return $"Command [{method.Name}] executed";
    }
}