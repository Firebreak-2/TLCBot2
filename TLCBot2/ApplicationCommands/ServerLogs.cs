using Discord;
using Discord.Interactions;
using Humanizer;
using TLCBot2.Core;
using TLCBot2.Data;
using TLCBot2.Logging;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [Group("server-logs", "Get and use logs from the server's database")]
    public class ServerLogsGroup : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("documentation", "Gives you the link for the documentation of the query language")]
        public async Task HelpSubcommand()
        {
            await RespondAsync("no link yet", ephemeral: true);
        }

        [SlashCommand("query", "Search through the server logs with the given filters")]
        public async Task SearchComplexSubcommand(string query, bool ephemeral = true)
        {
            try
            {
                var data = SearchQueryData.FromQuery(query);

                string results = "";
                string devModeAddition = $"```\n" +
                                         $"names  : {string.Join(" | ", data.FilteredNames)}\n" +
                                         $"tags   : {string.Join(" | ", data.FilteredTags.Select(x => $"[{string.Join(" & ", x)}]"))}\n" +
                                         $"times  : {string.Join(" | ", data.FilteredTimes)}\n" +
                                         $"returns: {string.Join(", ", data.ReturnData)}\n" +
                                         $"limit  : {data.ResultLimit}\n" +
                                         $"```\n";

                if (Program.DeveloperMode)
                    results += devModeAddition;

                results += $"```\n" +
                           $"{string.Join("```\n```\n", await data.Search())}\n" +
                           $"```";

                if (Context.User.OnMobile() && results.Length <= 2000)
                {
                    await RespondAsync(results, ephemeral: true);
                }
                else
                {
                    string[] resultsArray = await data.Search();

                    const string line = "--#x----------------";

                    results = string.Join('\n', resultsArray
                        .Select((x, i) => $"{line.Replace("x", (i + 1).ToString())}\n{x}\n"));

                    await Helper.UsingStreamFromText(results, async stream =>
                        await RespondWithFileAsync(
                            new FileAttachment(stream, "query.txt"),
                            devModeAddition.Length == 0
                                ? "Query Result"
                                : devModeAddition,
                            ephemeral: true)
                    );
                }
            }
            catch (Exception e)
            {
                await RespondAsync($"ERROR: {e.Message}".ToCodeBlock(), ephemeral: ephemeral);
            }
        }

        public record SearchQueryData(
            List<List<string>> FilteredTags,
            List<string> FilteredNames,
            List<string> FilteredImportances,
            List<string> FilteredTimes,
            List<string> ReturnData,
            int ResultLimit)
        {
            public async Task<string[]> Search()
            {
                await using var db = new TlcDbContext();
                string[] results = db.Logs
                    .OrderByDescending(x => x.ID)
                    .ToArray()
                    .Where(x => CheckIfLogEntryPassesFilters(x, this))
                    .Select(x =>
                    {
                        List<string> displayData = new();

                        foreach (string field in ReturnData)
                        {
                            switch (field.ToLower())
                            {
                                case "name":
                                    displayData.Add("Name: " + x.EventName);
                                    break;
                                case "tags":
                                    displayData.Add("Tags: " + x.Tags);
                                    break;
                                case "data":
                                    displayData.Add("Data: " + x.Data);
                                    break;
                                case "time":
                                    displayData.Add("Time: " + x.TimeStamp);
                                    break;
                                case "message":
                                    displayData.Add("Message: " + x.Message);
                                    break;
                                case "importance":
                                    displayData.Add("Importance: " + ((Log.Importance) x.Importance).Humanize());
                                    break;
                                case "id":
                                    displayData.Add("ID: " + x.ID);
                                    break;
                            }
                        }

                        return string.Join('\n', displayData);
                    })
                    .Take(ResultLimit)
                    .ToArray();
                return results;
            }

            public static SearchQueryData FromQuery(string query)
            {
                //* list -> tag multiples -> tag
                //  each item in the list is a multiple of
                //  tags that will have to all be present
                //  to return true. but if any multiple is true
                //  then their filtering will happen/work together
                List<List<string>> filteredTags = new();
                List<string> filteredNames = new();
                List<string> filteredImportances = new();
                List<string> filteredTimes = new();

                List<string> returnData = new();
                int resultLimit = 10;

                string[] split = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    string segment = split[i];

                    switch (segment.ToLower())
                    {
                        case "return":
                        {
                            Return: ;
                            if (split.ElementAtOrDefault(i + 1) is not { } dataItem)
                            {
                                if (segment.ToLower() == "and")
                                    throw new Exception("Expected 'and' item for 'return' pattern");

                                if (!returnData.Any())
                                    returnData.AddRange(new[] {"time", "message"});

                                break;
                            }

                            switch (dataItem.ToLower())
                            {
                                case "limit":
                                    returnData.AddRange(new[] {"time", "message"});
                                    break;
                                case "name":
                                case "tags":
                                case "data":
                                case "time":
                                case "message":
                                case "importance":
                                case "id":
                                {
                                    if (returnData.Contains(dataItem.ToLower()))
                                        throw new Exception($"Duplicate return field: {dataItem}");

                                    i++;
                                    returnData.Add(dataItem.ToLower());
                                    break;
                                }
                                default:
                                    throw new Exception($"Return field [{dataItem}] does not exist");
                            }

                            if (split.ElementAtOrDefault(i + 1) is { } nextElement)
                            {
                                i++;
                                switch (nextElement.ToLower())
                                {
                                    case "limit":
                                    {
                                        if (split.ElementAtOrDefault(++i) is not { } limitItem)
                                            throw new Exception("Expected 'limit' item for 'return' pattern");

                                        if (!int.TryParse(limitItem, out int limit))
                                            throw new Exception($"[{limitItem}] is not a valid number");

                                        switch (limit)
                                        {
                                            //! case > 25:
                                            //!     throw new Exception("Limit cannot be larger than `25`");
                                            case < 1:
                                                throw new Exception("Limit cannot be less than `1`");
                                        }

                                        resultLimit = limit;
                                        break;
                                    }
                                    case "and":
                                    {
                                        goto Return;
                                    }
                                    default:
                                        i--;
                                        break;
                                }
                            }

                            break;
                        }
                        case "with":
                        {
                            if (split.ElementAtOrDefault(++i) is not { } withItem)
                                throw new Exception("Expected 'with' pattern item");

                            switch (withItem.ToLower())
                            {
                                case "name":
                                case "names":
                                {
                                    if (split.ElementAtOrDefault(++i) is not { } nameFilter)
                                        throw new Exception("Expected event-name filter");

                                    filteredNames.Add(nameFilter);

                                    if (split.ElementAtOrDefault(i + 1) is { } nextElement
                                        && nextElement.ToLower() == "or")
                                    {
                                        i++;
                                        goto case "name";
                                    }

                                    break;
                                }
                                case "importance":
                                case "importances":
                                {
                                    if (split.ElementAtOrDefault(++i) is not { } importanceFilter)
                                        throw new Exception("Expected event-importance filter");

                                    // i should probably turn this into a regex
                                    // ^(?:[<>]=?|!=|=)$
                                    if (importanceFilter is ">" or ">=" or "<" or "<=" or "!=" or "=")
                                    {
                                        if (split.ElementAtOrDefault(++i) is not { } evaluationFilter)
                                            throw new Exception("Expected event-importance evaluation filter");

                                        filteredImportances.Add($"{importanceFilter};{evaluationFilter}");
                                    }
                                    else
                                    {
                                        filteredImportances.Add($"=;{importanceFilter}");
                                    }

                                    if (split.ElementAtOrDefault(i + 1) is { } nextElement
                                        && nextElement.ToLower() == "or")
                                    {
                                        i++;
                                        goto case "importance";
                                    }

                                    break;
                                }
                                case "tag":
                                case "tags":
                                {
                                    Tag: ;
                                    if (split.ElementAtOrDefault(++i) is not { } tagFilter)
                                        throw new Exception("Expected event-tag filter");

                                    List<string> filterSegment = new() {tagFilter};

                                    CheckChaining: ;
                                    if (split.ElementAtOrDefault(i + 1) is { } nextElement)
                                    {
                                        i++;
                                        switch (nextElement.ToLower())
                                        {
                                            case "and":
                                            {
                                                if (split.ElementAtOrDefault(++i) is not { } andItem)
                                                    throw new Exception(
                                                        $"Expected 'and' item for '{withItem.ToLower()}' pattern");

                                                filterSegment.Add(andItem);
                                                goto CheckChaining;
                                            }
                                            case "or":
                                            {
                                                filteredTags.Add(filterSegment);
                                                goto Tag;
                                            }
                                            default:
                                            {
                                                i--;
                                                filteredTags.Add(filterSegment);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        filteredTags.Add(filterSegment);
                                    }

                                    break;
                                }
                                case "time":
                                case "datetime":
                                {
                                    if (split.ElementAtOrDefault(++i) is not { } temporalPreposition)
                                        throw new Exception(
                                            $"Expected temporal preposition for '{withItem.ToLower()}' pattern");

                                    bool isDuration = temporalPreposition.ToLower() == "during";

                                    if (split.ElementAtOrDefault(++i) is not { } timeDuration)
                                        throw new Exception(
                                            $"Expected {(isDuration ? "duration" : "time")} for '{withItem.ToLower()}' pattern");

                                    if (!isDuration)
                                    {
                                        if (!long.TryParse(timeDuration, out long unix))
                                            throw new Exception(
                                                "Given time is not a number describing a UNIX timestamp");

                                        // will throw an exception if the time is invalid
                                        DateTimeOffset.FromUnixTimeSeconds(unix);

                                        filteredTimes.Add(temporalPreposition switch
                                        {
                                            "after" => $">;{unix}",
                                            "before" => $"<;{unix}",
                                            _ => throw new Exception("something went wrong...")
                                        });
                                    }
                                    else
                                    {
                                        string[] duringSplit = timeDuration.Split('-');

                                        if (!long.TryParse(duringSplit[0], out long after))
                                            throw new Exception(
                                                $"Given time [{duringSplit[0]}] is not a number describing a UNIX timestamp");

                                        if (!long.TryParse(duringSplit[1], out long before))
                                            throw new Exception(
                                                $"Given time [{duringSplit[1]}] is not a number describing a UNIX timestamp");

                                        filteredTimes.Add($"-;{after};{before}");
                                    }

                                    if (split.ElementAtOrDefault(i + 1) is { } nextElement
                                        && nextElement.ToLower() == "or")
                                    {
                                        i++;
                                        goto case "time";
                                    }

                                    break;
                                }
                                default:
                                    throw new Exception($"Unrecognized search filter: {withItem}");
                            }

                            break;
                        }
                        default:
                            throw new Exception($"Unrecognized keyword: {segment}");
                    }
                }

                return new SearchQueryData(filteredTags, filteredNames, filteredImportances, filteredTimes, returnData,
                    resultLimit);
            }
        }

        public static bool CheckIfLogEntryPassesFilters(
            LogEntry entry,
            SearchQueryData queryData) =>
            CheckIfLogEntryPassesFilters(entry,
                queryData.FilteredTags,
                queryData.FilteredNames,
                queryData.FilteredImportances,
                queryData.FilteredTimes);
        
        public static bool CheckIfLogEntryPassesFilters(
            LogEntry entry,
            List<List<string>>? filteredTags,
            List<string>? filteredNames,
            List<string>? filteredImportances,
            List<string>? filteredTimes)
        {
            filteredTags ??= new(0);
            filteredNames ??= new(0);
            filteredImportances ??= new(0);
            filteredTimes ??= new(0);
            
            {
                // true if no filter, false if some filter exists
                bool proceed = !filteredImportances.Any();

                foreach (string filteredImportance in filteredImportances)
                {
                    (string expression, string importance) =
                        filteredImportance.Split(';') is {Length: 2} split
                            ? (split[0], split[1])
                            : throw new Exception("Cannot have more than two expressions");

                    int importanceValue = (int) Enum.Parse<Log.Importance>(importance, true);
                    switch (expression)
                    {
                        case ">":
                            if (entry.Importance > importanceValue)
                                proceed = true;
                            break;
                        case ">=":
                            if (entry.Importance >= importanceValue)
                                proceed = true;
                            break;
                        case "<":
                            if (entry.Importance < importanceValue)
                                proceed = true;
                            break;
                        case "<=":
                            if (entry.Importance <= importanceValue)
                                proceed = true;
                            break;
                        case "!=":
                            if (entry.Importance != importanceValue)
                                proceed = true;
                            break;
                        case "=":
                            if (entry.Importance == importanceValue)
                                proceed = true;
                            break;
                    }
                }

                if (!proceed)
                    return false;
            }

            foreach (string name in filteredNames)
            {
                bool inverse = name[0] == '!';

                if ((inverse || !entry.EventName.CaselessEquals(name)) &&
                    (!inverse || entry.EventName.CaselessEquals(name[1..])))
                    return false;
            }

            foreach (var tagMultiple in filteredTags)
            {
                string[] eventTags = entry.Tags
                    .FromJson<string[]>()!
                    .Select(t => t.ToLower())
                    .ToArray();

                if (!tagMultiple.All(t =>
                    {
                        bool inverse = t[0] == '!';

                        return (!inverse && eventTags.Contains(t.ToLower()))
                               || (inverse && !eventTags.Contains(t[1..].ToLower()));
                    }))
                    return false;
            }

            foreach (string timeString in filteredTimes)
            {
                string[] split = timeString.Split(';')[1..];

                switch (timeString[0])
                {
                    case '>':
                        if (entry.TimeStamp <= split[0].To<long>())
                            return false;
                        break;
                    case '<':
                        if (entry.TimeStamp >= split[0].To<long>())
                            return false;
                        break;
                    case '-':
                        if (entry.TimeStamp < split[0].To<long>() || entry.TimeStamp > split[1].To<long>())
                            return false;
                        break;
                }
            }

            return true;
        }
    }
}