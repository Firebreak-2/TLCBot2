using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using MoreLinq;
using Newtonsoft.Json.Linq;
using TLCBot2.CommandLine;
using TLCBot2.CommandLine.Commands;
using TLCBot2.MessageBuilder.DiscordActions;
using TLCBot2.Utilities;

namespace TLCBot2.Types;

public record MessageComponentAction
{
    public string? ActionId = null;
    public Dictionary<string, object?>? Parameters = null;
    public Dictionary<string, object?> Variables = new();

    public DiscordActionResult<T> Execute<T>(object? executor, params (string, object)[] addedParameters)
    {
        Dictionary<string, object?> extendedParams = Variables.ToDictionary(variable => variable.Key, variable => variable.Value);

        foreach ((string key, object? value) in addedParameters)
        {
            extendedParams.Add(key, value);
        }
        
        if (ActionId is null)
            return new DiscordActionResult<T>(new Exception("Action is null"));

        if (!DiscordMethods.AllActions.TryFirst(x => x.Key.CaselessEquals(ActionId), out var methodAttributePair))
            return new DiscordActionResult<T>(new Exception($"Action [{ActionId}] does not exist"));

        var (_, (method, _)) = methodAttributePair;
        
        var parameters = method.Parameters;
        object?[]? deserializedParameters = null;
        
        if (Parameters is not null)
        {
            deserializedParameters = new object[parameters!.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                object? obj = Parameters.First(x => x.Key.CaselessEquals(parameters[i].Name)).Value;
                var conversionType = parameters[i].Type;

                deserializedParameters[i] = materialize(obj, conversionType);
            }
        }

        return DiscordActionResult<T>.From(() => (T) method.Invoke(deserializedParameters)!);
    
        object? materialize(object? obj, Type expectedType)
        {
            return obj switch
            {
                JObject jObj => jObj.ToObject(expectedType),
                JArray jArray => jArray.ToObject(expectedType),
                string s => TryManageSpecialStringsInString(s, executor, out object o) 
                    ? o
                    : extendedParams.TryGetValue(s, out object? val) 
                        ? materialize(val, expectedType) 
                        : Helper.ConvertFromString(s, expectedType),
                _ => expectedType == typeof(string) 
                    ? obj?.ToString() 
                    : Convert.ChangeType(obj, expectedType)
            };
        }
    }

    public static bool TryManageSpecialStringsInString(string str, object? origin, out object returnObject)
    {
        returnObject = null;
        
        var matches = _extractSpecialString.Matches(str);

        if (!matches.Any())
            return false;

        if (matches.Count > 1 && matches.First().Value.Length != str.Length)
            returnObject = _extractSpecialString.Replace(str, m => ParseSpecialString(m.Value, origin).ToString() ?? "null");
        else
            return TryParseSpecialString(str, origin, out returnObject);
        
        return true;
    }
    
    private static bool TryParseSpecialString(string specialString, object? origin, out object returnObject)
    {
        returnObject = null;
            
        try
        {
            returnObject = ParseSpecialString(specialString, origin);
            return true;
        }
        catch
        {
            // ignore
        }

        return false;
    }

    private static readonly Regex _extractSpecialString = new(@"\$%(.+?)%", RegexOptions.Compiled);

    private static object ParseSpecialString(string specialString, object? origin)
    {
        // this is going to be EXCEPTIONALLY slow from all the
        // unoptimized reflection and uncached regex, but im feeling
        // very lazy to fix that and for a discord bot it should be ok

        var match = _extractSpecialString.Match(specialString);
        
        if (!match.Success)
            throw new Exception("Invalid special string");

        // "$%[..]%"
        string str = match.Value;

        object? currentObject = null;

        string[] matches = _specialStringTokenizer.Matches(str).Select(x => x.Value).ToArray();
        for (int i = 0; i < matches.Length; i++)
        {
            string segment = matches[i];
            
            if (currentObject is null)
            {
                currentObject = segment switch
                {
                    "this" => origin,
                    "null" => null,
                    _ => throw new Exception($"Unrecognized keyword when parsing special string: {segment}")
                };
                continue;
            }

            if (segment == ".")
            {
                if (matches.ElementAtOrDefault(++i) is not { } innerMember)
                    throw new Exception("Identifier expected");
                
                var type = currentObject.GetType();
                    
                var potentialMembers = type.GetMember(
                        innerMember,
                        BindingFlags.NonPublic
                        | BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.IgnoreCase)
                    .Where(x => x.MemberType 
                        is MemberTypes.Field 
                        or MemberTypes.Property
                        or MemberTypes.Method)
                    .ToArray();

                if (!potentialMembers.Any())
                    throw new Exception($"No members for type [{type.Name}] with the name [{innerMember}] exist");

                currentObject = potentialMembers[0] switch
                {
                    PropertyInfo info => info.GetValue(currentObject),
                    FieldInfo info => info.GetValue(currentObject),
                    MethodInfo info => (info, currentObject),
                    _ => throw new NotImplementedException()
                };
                
                continue;
            }

            var match2 = _extractParameters.Match(segment);
            if (match2.Success)
            {
                string v = match2.Value;
                
                switch (segment[0])
                {
                    case '[': // ienumerable
                    {
                        currentObject = currentObject switch
                        {
                            object?[] arr => arr[v.To<int>()],
                            Dictionary<string, object?> dic => dic[v],
                            _ => throw new NotImplementedException(
                                $"IEnumerable type [{currentObject.GetType().Name}] is not yet implemented")
                        };
                        break;
                    }
                    case '(': // method
                    {
                        throw new Exception("Method calling is not supported");
                        // had to disable this to prevent remote code execution
                        // sucks but better safe than sorry :(
                    
                        var x = (ValueTuple<MethodInfo, object?>) currentObject;
                        object?[]? parameters = ChannelTerminal.GetParametersFromArguments(x.Item1,
                            v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                        
                        currentObject = x.Item1.Invoke(x.Item2, parameters);
                        break;
                    }
                }

                continue;
            }
        }
        
        return currentObject ?? throw new InvalidOperationException();
    }

    private static readonly Regex _specialStringTokenizer =
        new(@"(?:(?=(?:^|\w) *)[a-zA-Z_]\w*|(?<!^)(?:\.|\[.+?\]|\(.*?\)))", RegexOptions.Compiled);
    
    private static readonly Regex _extractParameters =
        new(@"(?<=\().*?(?=\))|(?<=\[).*?(?=\])", RegexOptions.Compiled);
}