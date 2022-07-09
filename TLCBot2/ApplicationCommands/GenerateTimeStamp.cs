using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Humanizer;
using TLCBot2.Utilities;

namespace TLCBot2.ApplicationCommands;

public partial class InteractionCommands
{
    [Group("generate-timestamp", "Generates a dynamic timestamp for use")]
    public class GenerateTimeStamp : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("with-offset", "Allows you to manually input which UTC offset to use for the timestamp generation")]
        public async Task Manually(
            [MinValue(-14)] [MaxValue(14)] int utcOffset,
            [MinValue(0)] [MaxValue(9999)] int? year = null,
            [MinValue(0)] [MaxValue(12)] int? month = null,
            [MinValue(0)] [MaxValue(31)] int? day = null,
            [MinValue(0)] [MaxValue(23)] int? hour = null,
            [MinValue(0)] [MaxValue(59)] int? minute = null,
            [MinValue(0)] [MaxValue(59)] int? second = null,
            Helper.DynamicTimestampFormat style = Helper.DynamicTimestampFormat.ShortDateTime)
        {
            await RespondAsync(GetTimestampString(
                utcOffset,
                year,
                month,
                day,
                hour,
                minute,
                second,
                style));
        }
        
        [SlashCommand("automatic", "Gives you a prompt to find your UTC offset and then generates the timestamp")]
        public async Task Automatic(
            int? year = null,
            int? month = null,
            int? day = null,
            int? hour = null,
            int? minute = null,
            int? second = null,
            Helper.DynamicTimestampFormat style = Helper.DynamicTimestampFormat.ShortDateTime)
        {
            var now = DateTimeOffset.UtcNow - DateTimeOffset.Now.Offset;
            long nowUnix = now.ToUnixTimeSeconds();
            string timestamp = now.ToDynamicTimestamp(Helper.DynamicTimestampFormat.LongDateTime);
            await RespondAsync($"If it's currently {timestamp} for you right now, press CONFIRM",
                components: new ComponentBuilder()
                    .WithButton("-1 Hour", $"time-gen-button;dec,{nowUnix},{(char) (int) style}", ButtonStyle.Secondary)
                    .WithButton("Cancel",  $"time-gen-button;cancel,{nowUnix},{(char) (int) style}", ButtonStyle.Danger)
                    .WithButton("Confirm", $"time-gen-button;confirm,{nowUnix},{(char) (int) style}", ButtonStyle.Success)
                    .WithButton("+1 Hour", $"time-gen-button;inc,{nowUnix},{(char) (int) style}", ButtonStyle.Secondary)
                    .Build());
        }
    }

    [ComponentInteraction("time-gen-button;*,*,*")]
    public async Task DecrementButtonResponse(string action, string initialTimeUnix, string format)
    {
        var interaction = (SocketMessageComponent) Context.Interaction;
        if (interaction.Message.Interaction.User.Id != interaction.User.Id)
            return;

        var initialTime = DateTimeOffset.FromUnixTimeSeconds(initialTimeUnix.To<long>());
        if (Helper.DateTimeOffsetFromDynamicTimestamp(interaction.Message.Content)
            is not var (currentTime, _))
            return;
        int offset = currentTime.Hour - initialTime.Hour;

        switch (action)
        {
            case "inc":
            case "dec":
            {
                int addition = action == "inc" ? 1 : -1;
                currentTime += addition.Hours();
                offset += addition;
                string offsetString = offset < 0 ? offset.ToString() : $"+{offset}";
                string timestamp = currentTime.ToDynamicTimestamp(Helper.DynamicTimestampFormat.LongDateTime);
                string newContent = $"UTC{offsetString} {timestamp}";
                await interaction.Message.ModifyAsync(props => props.Content = newContent);
                break;
            }
            case "confirm":
            {
                initialTime += DateTimeOffset.Now.Offset;
                await interaction.Message.ModifyAsync(props =>
                {
                    props.Content = GetTimestampString(
                        offset,
                        initialTime.Year, 
                        initialTime.Month,
                        initialTime.Day,
                        initialTime.Hour,
                        initialTime.Minute,
                        initialTime.Second,
                        (Helper.DynamicTimestampFormat) format[0]);
                    props.Components = new ComponentBuilder().Build();
                });
                break;
            }
            case "cancel":
            {
                await interaction.Message.DeleteAsync();
                break;
            }
        }
        
        await DeferAsync();
    }
    
    private static string GetTimestampString(
        int utcOffset,
        int? year = null,
        int? month = null,
        int? day = null,
        int? hour = null,
        int? minute = null,
        int? second = null,
        Helper.DynamicTimestampFormat style = Helper.DynamicTimestampFormat.ShortDateTime)
    {
        var offset = utcOffset.Hours();
        var now = DateTimeOffset.UtcNow + offset;
            
        var time = new DateTime(
            year ?? now.Year,
            month ?? now.Month,
            day ?? now.Day,
            hour ?? now.Hour,
            minute ?? now.Minute,
            second ?? now.Second);
                    
        string timestamp = new DateTimeOffset(time).ToDynamicTimestamp(style);
        return $"{timestamp} `{timestamp}`";
    }
}