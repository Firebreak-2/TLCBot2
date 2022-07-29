using Discord;
using TLCBot2.Core;
using TLCBot2.Utilities;

namespace TLCBot2.Types;

public record MessageData
{
    public string? Text = null;
    public EmbedData? Embed = null;
    public string? ReferenceJumpUrl = null;
    public List<List<MessageComponentsData.ComponentData>>? ComponentRows = null;

    public async Task<bool> SendAsync(ulong sendChannel)
    {
        if (await Helper.GetChannelFromIdAsync(sendChannel) is not { } channel)
            return false;

        await channel.SendMessageAsync(
            text: this.Text ?? "",
            embed: this.Embed?.ToEmbed(),
            messageReference: this.ReferenceJumpUrl is { }
                ? Helper.MessageReferenceFromJumpUrl(this.ReferenceJumpUrl)
                : null,
            components: MessageComponentsData.RowsToMessageComponent(ComponentRows)
        );

        return true;
    }

    public static class MessageComponentsData
    {
        public record ComponentData(string ComponentType)
        {
            public string? ButtonLabel = null;
            public string? ButtonColor = null;

            public string? SelectMenuPlaceholder = null;
            public int? SelectMenuMinValues = null;
            public int? SelectMenuMaxValues = null;
            public List<SelectMenuOptionData>? SelectMenuOptions = null;

            public string ComponentType = ComponentType;
            public string? ActionLink = null;
            public bool? IsDisabled = null;

            public const string ButtonComponentType = "Button";
            public const string SelectMenuComponentType = "SelectMenu";

            public IMessageComponent ToMessageComponent()
            {
                switch (ComponentType)
                {
                    case "Button":
                    {
                        var builder = new ButtonBuilder();

                        (var _, ulong channelId, ulong messageId) = 
                            Helper.MessageInfoFromJumpUrl(ActionLink ?? "0/0/0");

                        string actionLinkSuffix = ActionLink is { }
                            ? $"{channelId}/{messageId}"
                            : "null";
                        
                        builder.WithCustomId($"msgbuild-{ComponentType}-{actionLinkSuffix}");

                        if (ButtonLabel is not null)
                            builder.WithLabel(ButtonLabel);
                        builder.WithStyle(ButtonColor is not null
                            ? ButtonStyleColor.FromString(ButtonColor)
                            : ButtonStyle.Primary);
                        if (IsDisabled is not null)
                            builder.WithDisabled(IsDisabled.Value);

                        return builder.Build();
                    }
                    case "SelectMenu":
                    {
                        var builder = new SelectMenuBuilder();
                        
                        (var _, ulong channelId, ulong messageId) = 
                            Helper.MessageInfoFromJumpUrl(ActionLink ?? "0/0/0");
                        
                        string actionLinkSuffix = ActionLink is { }
                            ? $"{channelId}/{messageId}"
                            : "null";
                        
                        builder.WithCustomId($"msgbuild-{ComponentType}-{actionLinkSuffix}");

                        if (SelectMenuPlaceholder is not null)
                            builder.WithPlaceholder(SelectMenuPlaceholder);
                        if (SelectMenuMinValues is not null)
                            builder.WithMinValues(SelectMenuMinValues.Value);
                        if (SelectMenuMaxValues is not null)
                            builder.WithMaxValues(SelectMenuMaxValues.Value);
                        if (SelectMenuOptions is not null)
                            builder.WithOptions(SelectMenuOptions
                                .Select(x => x.ToSelectMenuOptionBuilder())
                                .ToList());

                        return builder.Build();
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public record SelectMenuOptionData
            {
                public string? Label = null;
                public string? Value = null;
                public string? Description = null;
                public bool? IsDefault = null;

                public SelectMenuOptionBuilder ToSelectMenuOptionBuilder()
                {
                    var builder = new SelectMenuOptionBuilder();

                    if (Label is not null)
                    {
                        builder.WithLabel(Label);
                        builder.WithValue(Value ?? Label.ToLower().Replace(" ", ""));
                    }

                    if (Description is not null)
                        builder.WithDescription(Description);
                    if (IsDefault is not null)
                        builder.WithDefault(IsDefault.Value);

                    return builder;
                }
            }

            public static class ButtonStyleColor
            {
                public static ButtonStyle FromString(string color) =>
                    color switch
                    {
                        "Blue" => ButtonStyle.Primary,
                        "Gray" => ButtonStyle.Secondary,
                        "Green" => ButtonStyle.Success,
                        "Red" => ButtonStyle.Danger,
                        _ => default
                    };

                public static string FromButtonStyle(ButtonStyle style) =>
                    style switch
                    {
                        ButtonStyle.Primary => "Blue",
                        ButtonStyle.Secondary => "Gray",
                        ButtonStyle.Success => "Green",
                        ButtonStyle.Danger => "Red",
                        _ => "null"
                    };
            }
        }

        public static MessageComponent? RowsToMessageComponent(List<List<ComponentData>>? rows)
        {
            if (rows is null)
                return null;
            
            ComponentBuilder componentBuilder = new();
            foreach (var row in rows)
            {
                var rowBuilder = new ActionRowBuilder();
                foreach (var component in row)
                {
                    rowBuilder.AddComponent(component.ToMessageComponent());
                }

                componentBuilder.AddRow(rowBuilder);
            }

            return componentBuilder.Build();
        }
    }

    public record EmbedData
    {
        public ColorData? Color = null;
        public ulong? Author = null;
        public string? Description = null;
        public List<FieldData>? Fields = null;
        public string? Footer = null;
        public string? ImageUrl = null;
        public long? UnixTimestamp = null;
        public string? Title = null;

        public Embed ToEmbed()
        {
            var builder = new EmbedBuilder();

            if (Color is not null)
                builder.WithColor(Color.ToColor());
            if (Author is not null)
                builder.WithAuthor(Program.Client.GetUser(Author.Value));
            if (Description is not null)
                builder.WithDescription(Description);
            if (Fields is not null)
                builder.WithFields(Fields.Select(x => x.ToField()));
            if (ImageUrl is not null)
                builder.WithImageUrl(ImageUrl);
            if (UnixTimestamp is not null)
                builder.WithTimestamp(DateTimeOffset.FromUnixTimeSeconds(UnixTimestamp.Value));
            if (Title is not null)
                builder.WithTitle(Title);

            return builder.Build();
        }

        public record ColorData
        {
            public float Red;
            public float Green;
            public float Blue;

            public Color ToColor() =>
                new(Red / 255f, Green / 255f, Blue / 255f);
        }

        public record FieldData
        {
            public string? Name = null;
            public string? Value = null;

            public EmbedFieldBuilder ToField()
            {
                var builder = new EmbedFieldBuilder();

                if (Name is { })
                    builder.WithName(Name);
                if (Value is { })
                    builder.WithValue(Value);

                return builder;
            }
        }
    }
}