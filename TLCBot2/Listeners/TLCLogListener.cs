using Discord;
using Discord.WebSocket;
using Humanizer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TLCBot2.Core;
using TLCBot2.Utilities;
using Color = Discord.Color;

namespace TLCBot2.Listeners;

public static class TLCLogListener
{
    public static void PreInitialize()
    {
        Program.Client.UserCommandExecuted += OnUserCommandExecuted;
        Program.Client.MessageCommandExecuted += OnMessageCommandExecuted;
        Program.Client.SelectMenuExecuted += OnSelectionMenuExecuted;
        Program.Client.ButtonExecuted += OnButtonExecuted;
        Program.Client.ModalSubmitted += OnModalSubmitted;
        Program.Client.SlashCommandExecuted += OnSlashCommandExecuted;
        Program.Client.ChannelCreated += OnChannelCreated;
        Program.Client.ChannelDestroyed += OnChannelDeleted;
        Program.Client.ChannelUpdated += OnChannelUpdated;
        Program.Client.GuildStickerCreated += OnStickerCreated;
        Program.Client.GuildStickerDeleted += OnStickerDeleted;
        Program.Client.GuildStickerUpdated += OnStickerUpdated;
        Program.Client.InviteCreated += OnInviteCreated;
        Program.Client.InviteDeleted += OnInviteDeleted;
        Program.Client.MessageDeleted += OnMessageDeleted;
        Program.Client.MessagesBulkDeleted += OnBulkMessagesDeleted;
        Program.Client.MessageUpdated += OnMessageUpdated;
        Program.Client.PresenceUpdated += OnPresenceUpdated;
        Program.Client.ReactionAdded += OnReactionAdded;
        Program.Client.ReactionRemoved += OnReactionRemoved;
        Program.Client.ReactionsCleared += OnReactionsCleared;
        Program.Client.ReactionsRemovedForEmote += OnReactionsClearedForEmote;
        Program.Client.RoleCreated += OnRoleCreated;
        Program.Client.RoleDeleted += OnRoleDeleted;
        Program.Client.RoleUpdated += OnRoleUpdated;
        Program.Client.SpeakerAdded += OnStageSpeakerAdded;
        Program.Client.SpeakerRemoved += OnStageSpeakerRemoved;
        Program.Client.StageStarted += OnStageStarted;
        Program.Client.StageEnded += OnStageEnded;
        Program.Client.StageUpdated += OnStageUpdated;
        Program.Client.ThreadCreated += OnThreadCreated;
        Program.Client.ThreadDeleted += OnThreadDeleted;
        Program.Client.ThreadUpdated += OnThreadUpdated;
        Program.Client.ThreadMemberJoined += OnThreadUserJoined;
        Program.Client.ThreadMemberLeft += OnThreadUserLeft;
        // Program.Client.UserBanned += 
        // Program.Client.UserUnbanned += 
        // Program.Client.UserJoined += 
        // Program.Client.UserLeft += 
        // Program.Client.UserUpdated += 
        // Program.Client.UserVoiceStateUpdated += 
        // Program.Client.GuildScheduledEventCancelled += 
        // Program.Client.GuildScheduledEventCompleted += 
        // Program.Client.GuildScheduledEventCreated += 
        // Program.Client.GuildScheduledEventStarted += 
        // Program.Client.GuildScheduledEventUpdated += 
        // Program.Client.GuildScheduledEventUserAdd += 
        // Program.Client.GuildScheduledEventUserRemove += 
        // Program.Client.GuildMemberUpdated += 
        // Program.Client.ThreadMemberJoined += 
        // Program.Client.ThreadMemberLeft += 
        // Program.Client.GuildJoinRequestDeleted += 
    }

    public static Task OnThreadUserJoined(SocketThreadUser user) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"thread user joined", "threads"},
            "User Joined Thread",
            user,
            ("Thread", user.Thread.Mention));
    public static Task OnThreadUserLeft(SocketThreadUser user) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"thread user left", "threads"},
            "User Left Thread",
            user,
            ("Thread", user.Thread.Mention));
    public static Task OnThreadCreated(SocketThreadChannel thread) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"threads", "thread created", "server change"},
            "Thread Created",
            null,
            ("Thread Name", thread.Name),
            ("Thread Creator", thread.Owner.Username),
            ("Thread Channel", $"<#{thread.ParentChannel.Id}>"));

    public static Task OnThreadDeleted(Cacheable<SocketThreadChannel, ulong> threadCacheable)
    {
        if (threadCacheable.Value is not {} thread) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Neutral,
            new[] {"threads", "thread deleted", "server change"},
            "Thread Deleted",
            null,
            ("Thread Name", thread.Name),
            ("Thread Creator", thread.Owner.Username),
            ("Thread Channel", $"<#{thread.ParentChannel.Id}>"));
    }
    public static Task OnThreadUpdated(
        Cacheable<SocketThreadChannel, ulong> oldThreadCacheable,
        SocketThreadChannel newThread)
    {
        var oldThread = oldThreadCacheable.Value;
        var oldThreadData = (oldThread?.Name, oldThread?.Owner, oldThread?.ParentChannel);
        var newThreadData = (newThread.Name, newThread.Owner, newThread.ParentChannel);
        if (oldThreadData == newThreadData) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Neutral,
            new[] {"threads", "thread updated", "server change"},
            "Thread Updated",
            null,
            ("Thread", $"{oldThreadData.Name ?? "Null"} → {newThreadData.Name}"),
            ("Thread Creator", $"{oldThreadData.Owner?.Username ?? "Null"} → {newThreadData.Owner.Username}"),
            ("Thread Channel", $"<#{oldThreadData.ParentChannel?.Id ?? 0}> → <#{newThreadData.ParentChannel.Id}>"));
    }
    public static Task OnStageUpdated(SocketStageChannel oldChannel, SocketStageChannel newChannel)
    {
        var oldData = oldChannel.Topic;
        var newData = newChannel.Topic;
        if (oldData == newData) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Useless,
            new[] {"stage", "stage updated", "server change"},
            "Stage Updated",
            null,
            ("Topic", $"{oldData} → {newData}"));
    }
    public static Task OnStageStarted(SocketStageChannel channel) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"stage", "stage started", "server change"},
            "Stage Started",
            null,
            ("Stage", channel.Name),
            ("Topic", channel.Topic));
    public static Task OnStageEnded(SocketStageChannel channel) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"stage", "stage ended", "server change"},
            "Stage Ended",
            null,
            ("Stage", channel.Name),
            ("Topic", channel.Topic));
    public static Task OnStageSpeakerAdded(SocketStageChannel channel, SocketGuildUser user) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"stage", "user change", "speaker added"},
            "Stage Speaker Added",
            user,
            ("Stage Channel", channel.Name),
            ("Speakers", channel.Speakers.Select(x =>
                x.Username)),
            ("Topic", channel.Topic));
    public static Task OnStageSpeakerRemoved(SocketStageChannel channel, SocketGuildUser user) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"stage", "user change", "speaker removed"},
            "Stage Speaker Removed",
            user,
            ("Stage Channel", channel.Name),
            ("Speakers", channel.Speakers.Select(x =>
                x.Username)),
            ("Topic", channel.Topic));
    public static Task OnRoleCreated(SocketRole role) =>
        AddLogEntry(LogSignificance.Warning,
            new[] {"roles", "role created", "server change"},
            "Role Created",
            null,
            new (object, object)[]
            {
                ("Role Name", role.Name),
                ("Role Color Value", role.Color)
            }, new Func<string>(() =>
            {
                var col = role.Color.DiscordColorToArgb32();
                using var image = new Image<Argb32>(500, 500);
                image.FillColor(col);
                using var stream = image.ToStream();
                return Helper.GetFileUrl(stream);
            })());
    public static Task OnRoleDeleted(SocketRole role) =>
        AddLogEntry(LogSignificance.Dangerous,
            new[] {"roles", "role deleted", "server change"},
            "Role Deleted",
            null,
            new (object, object)[]
            {
                ("Role Name", role.Name),
                ("Role Color Value", role.Color)
            }, new Func<string>(() =>
            {
                using var image = new Image<Argb32>(500, 500);
                image.FillColor(role.Color.DiscordColorToArgb32());
                using var stream = image.ToStream();
                return Helper.GetFileUrl(stream);
            })());

    public static Task OnRoleUpdated(SocketRole oldRole, SocketRole newRole)
    {
        (string name, Color color) oldRoleData = (oldRole.Name, oldRole.Color); 
        (string name, Color color) newRoleData = (newRole.Name, newRole.Color); 
        if (oldRoleData == newRoleData) return Task.CompletedTask;
        
        var col1 = oldRole.Color.DiscordColorToArgb32();
        var col2 = newRole.Color.DiscordColorToArgb32();
        using var image = new Image<Argb32>(500, 500);
        image.FillColor((x, _) => x <= 250
            ? col1
            : col2);
        using var stream = image.ToStream();
        string imageUrl =  Helper.GetFileUrl(stream);
        
        return AddLogEntry(LogSignificance.Warning,
            new[] {"roles", "role updated", "server change"},
            "Role Updated",
            null,
            new (object, object)[]
            {
                ("Role Name", $"{oldRole.Name} → {newRole.Name}"),
                ("Role Color Value", $"{oldRole.Color} → {newRole.Color}")
            }, imageUrl);
    }

    public static Task OnReactionsCleared(
        Cacheable<IUserMessage, ulong> messageCacheable,
        Cacheable<IMessageChannel, ulong> channelCacheable)
    {
        var message = messageCacheable.GetOrDownloadAsync().Result;
        var channel = channelCacheable.GetOrDownloadAsync().Result;
        return AddLogEntry(LogSignificance.Neutral,
            new[] {"reactions", "reaction removed", "reactions cleared", "reactions cleared for message", "message change"},
            "Reactions Cleared For Message",
            null,
            ("Original Message", "Go to message".GetHyperLink(message.GetJumpUrl())),
            ("Channel", channel.Name));
    }
    public static Task OnReactionsClearedForEmote(
        Cacheable<IUserMessage, ulong> messageCacheable,
        Cacheable<IMessageChannel, ulong> channelCacheable,
        IEmote emote)
    {
        var message = messageCacheable.GetOrDownloadAsync().Result;
        var channel = channelCacheable.GetOrDownloadAsync().Result;
        return AddLogEntry(LogSignificance.Neutral,
            new[] {"reactions", "reaction removed", "reactions cleared", "reactions cleared for emote", "message change"},
            "Reactions Cleared For Emote",
            null,
            new (object, object)[] 
            {
                ("Reaction Emote Name", emote.Name),
                ("Reaction Message", "Go to message".GetHyperLink(message.GetJumpUrl())),
                ("Channel", channel.Name),
            }, emote is Emote customEmote ? customEmote.Url : null);
    }
    public static Task OnReactionRemoved(
        Cacheable<IUserMessage, ulong> messageCacheable,
        Cacheable<IMessageChannel, ulong> channelCacheable,
        SocketReaction reaction)
    {
        var message = messageCacheable.GetOrDownloadAsync().Result;
        var channel = channelCacheable.GetOrDownloadAsync().Result;
        return AddLogEntry(LogSignificance.Useless,
            new[] {"reactions", "reaction removed", "message change"},
            "Reaction Removed",
            Program.Client.GetUser(reaction.UserId),
            new (object, object)[] 
            {
                ("Reaction Emote Name", reaction.Emote.Name),
                ("Reaction Message", $"[Go to message]({message.GetJumpUrl()})"),
                ("Channel", channel.Name),
            }, reaction.Emote is Emote customEmote ? customEmote.Url : null);
    }
    public static Task OnReactionAdded(
        Cacheable<IUserMessage, ulong> messageCacheable,
        Cacheable<IMessageChannel, ulong> channelCacheable,
        SocketReaction reaction)
    {
        var message = messageCacheable.GetOrDownloadAsync().Result;
        var channel = channelCacheable.GetOrDownloadAsync().Result;
        return AddLogEntry(LogSignificance.Useless,
            new[] {"reactions", "reaction added", "message change"},
            "Reaction Added",
            Program.Client.GetUser(reaction.UserId),
            new (object, object)[] 
            {
                ("Reaction Emote Name", reaction.Emote.Name),
                ("Reaction Message", $"[Go to message]({message.GetJumpUrl()})"),
                ("Channel", channel.Name),
            }, reaction.Emote is Emote customEmote ? customEmote.Url : null);
    }

    public static Task OnPresenceUpdated(
        SocketUser user,
        SocketPresence oldPresence,
        SocketPresence newPresence) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"status changed", "user change", "status"},
            "Status Changed",
            user,
            ("Status", $"{oldPresence.Status.Humanize()} → {newPresence.Status.Humanize()}"));

    public static Task OnMessageUpdated(
        Cacheable<IMessage, ulong> oldMessageCacheable,
        SocketMessage newMessage,
        ISocketMessageChannel channel)
    {
        if (channel.Id == RuntimeConfig.TLCLogs.Id) return Task.CompletedTask;
        var oldMessage = oldMessageCacheable.Value;
        if (oldMessage is null) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Neutral,
            new[] {"message edited", "message change", $"channel={channel.Id}"},
            "Message Edited",
            Program.Client.GetUser(oldMessage.Author.Id),
            new (object, object)[]
            {
                ("Channel", ((SocketTextChannel)channel).Mention),
                ("Content", $"{oldMessage.Content} → {newMessage.Content}")
            },
            newMessage.Attachments.FirstOrDefault()?.Url);
    }
    public static Task OnBulkMessagesDeleted(
        IReadOnlyCollection<Cacheable<IMessage, ulong>> messagesCacheables,
        Cacheable<IMessageChannel, ulong> channelCacheable)
    {
        var messages = messagesCacheables.Select(x => x.Value);
        var channel = channelCacheable.Value;
        if (channel.Id == RuntimeConfig.TLCLogs.Id) return Task.CompletedTask;
        if (messages.Any(x => x is null)) return Task.CompletedTask;
        AddLogEntry(LogSignificance.Warning,
            new[] {"message deleted", "message change", "bulk messages deleted", $"channel={channel.Id}"},
            "Bulk Messages Deleted",
            null,
            ("Channel", ((SocketTextChannel)channel).Mention));
        using var stream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream);
        streamWriter.Write(messages.Select(x =>
            $"<{x.Author.Username}#{x.Author.Discriminator}> {x.CreatedAt.UtcDateTime.Humanize()} (UTC):\n{x.Content}\n\n"));
        streamWriter.Close();
        RuntimeConfig.TLCLogs.SendFileAsync(stream, "Messages", "Messages Deleted");
        return Task.CompletedTask;
    }
    public static Task OnMessageDeleted(
        Cacheable<IMessage, ulong> messageCacheable,
        Cacheable<IMessageChannel, ulong> channelCacheable)
    {
        var message = messageCacheable.Value;
        var channel = channelCacheable.Value;
        if (message is null || channel is null) return Task.CompletedTask;
        if (channel.Id == RuntimeConfig.TLCLogs.Id) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Warning,
            new[] {"message deleted", "message change"},
            "Message Deleted",
            Program.Client.GetUser(message.Author.Id),
            new (object, object)[] {
                ("Channel", ((SocketTextChannel)channel).Mention),
                ("Content", $"{message.Content}")
            }, message.Attachments.FirstOrDefault()?.Url);
    }
    public static Task OnInviteCreated(SocketInvite invite) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"invite created", "server change"},
            "Invite Created",
            invite.Inviter,
            ("Invite Channel", invite.Channel.Name),
            ("Target User", invite.TargetUser is {} targetUser ? targetUser.Mention : "null"),
            ("Invite Code", invite.Id));
    public static Task OnInviteDeleted(SocketGuildChannel inviteChannel, string oldInviteCode) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"invite deleted", "server change", $"channel={inviteChannel.Id}"},
            "Invite Deleted",
            null,
            ("Invite Channel", inviteChannel.Name),
            ("Invite Code", oldInviteCode));
    public static Task OnStickerCreated(SocketCustomSticker sticker) =>
        AddLogEntry(LogSignificance.Warning,
            new[] {"sticker created", "server change"},
            "Sticker Created",
            null,
            new (object, object)[] {("Sticker Name", sticker.Name),
            ("Description", sticker.Description),
            ("Sticker Type", sticker.Format.Humanize()),
            ("Creator", sticker.Author)}, sticker.GetStickerUrl());
    public static Task OnStickerDeleted(SocketCustomSticker sticker) =>
        AddLogEntry(LogSignificance.Warning,
            new[] {"sticker deleted", "server change"},
            "Sticker Created",
            null,
            new (object, object)[] {("Sticker Name", sticker.Name),
            ("Description", sticker.Description),
            ("Sticker Type", sticker.Format.Humanize()),
            ("Creator", sticker.Author)}, sticker.GetStickerUrl());
    public static Task OnStickerUpdated(SocketCustomSticker oldSticker, SocketCustomSticker newSticker) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"sticker updated", "server change"},
            "Sticker Created",
            null,
            new (object, object)[] {("Sticker Name", $"{oldSticker.Name} → {newSticker.Name}"),
            ("Description", $"{oldSticker.Description} → {newSticker.Description}"),
            ("Sticker Type", $"{oldSticker.Format.Humanize()} → {newSticker.Format.Humanize()}"),
            ("Creator", newSticker.Author)}, newSticker.GetStickerUrl());

    public static Task OnChannelUpdated(SocketChannel oldChannel, SocketChannel newChannel)
    {
        (string oldChannelName, string oldChannelType) = Helper.GetChannelInfo(oldChannel);
        (string newChannelName, string newChannelType) = Helper.GetChannelInfo(newChannel);
        return AddLogEntry(LogSignificance.Warning,
            new[] {"channel updated", "server change", $"channel={newChannel.Id}"},
            $"{oldChannelType} Updated",
            null,
            (oldChannelType, $"{oldChannelName} → {newChannelName} : {newChannel.Id}"),
            ("Channel Type Change", $"{oldChannelType} → {newChannelType}"),
            ("Current Channel Type", newChannel.GetChannelType().Humanize()));
    }
    public static Task OnChannelCreated(SocketChannel channel)
    {
        (string channelName, string channelType) = Helper.GetChannelInfo(channel);
        return AddLogEntry(LogSignificance.Warning,
            new[] {"channel created", "server change", $"channel={channel.Id}"},
            $"{channelType} Created",
            null,
            (channelType, $"{channelName} : {channel.Id}"),
            ("Channel Type", channel.GetChannelType().Humanize()));
    }
    public static Task OnChannelDeleted(SocketChannel channel)
    {
        (string channelName, string channelType) = Helper.GetChannelInfo(channel);
        return AddLogEntry(LogSignificance.Dangerous,
            new[] {"channel deleted", "server change", $"channel={channel.Id}"},
            $"{channelType} Deleted",
            null,
            (channelType, $"{channelName} : {channel.Id}"),
            ("Channel Type", channel.GetChannelType().Humanize()));
    }
    public static Task OnSlashCommandExecuted(SocketSlashCommand command) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"slash command executed", "command", "interaction"},
            "Slash Command Executed",
            command.User,
            ("Command Name", command.CommandName),
            ("Parameters Used",
                command.Data.Options is {Count: > 0}
                    ? string.Join("\n", command.Data.Options.Select(x => $"{x.Name}: {x.Value}"))
                    : "None"));
    public static Task OnModalSubmitted(SocketModal modal) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"modal submitted", "interaction"},
            "Modal Submitted",
            modal.User,
            ("Channel Of Modal", $"<#{modal.Channel.Id}>"),
            ("Parameters Used",
                modal.Data.Components is {Count: > 0}
                    ? string.Join("\n", modal.Data.Components.Select((x, i) => 
                        $"`{i + 1}` {x.Value}"))
                    : "None"));
    public static Task OnSelectionMenuExecuted(SocketMessageComponent selectionMenu) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"selection menu executed", "message component", "interaction"},
            "Selection Menu Executed",
            selectionMenu.User,
            ("Selected Value(s)",
                string.Join("\n", selectionMenu.Data.Values.Select((x, i) => 
                    $"`{i+1}` {x}"))),
            ("Original Message", $"[Go to message]({selectionMenu.Message.GetJumpUrl()})"),
            ("Unique ID", selectionMenu.Data.CustomId));
    public static Task OnButtonExecuted(SocketMessageComponent button) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"button executed", "message component", "interaction"},
            "Button Executed",
            button.User,
            ("Original Message", $"[Go to message]({button.Message.GetJumpUrl()})"),
            ("Unique ID", button.Data.CustomId));
    public static Task OnUserCommandExecuted(SocketUserCommand command) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"user command executed", "command", "interaction"},
            "User Command Executed", 
            command.User, 
            ("Command Name", command.CommandName),
            ("Used On", command.Data.Member.Mention));
    public static Task OnMessageCommandExecuted(SocketMessageCommand command) => 
        AddLogEntry(LogSignificance.Neutral,
            new[] {"message command executed", "command", "interaction"},
            "Message Command Executed",
            command.User,
            ("Command Name", command.CommandName),
            ("Message Used On", $"[Go to message]({command.Data.Message.GetJumpUrl()})"));
    public enum LogSignificance
    {
        Dangerous,
        Warning,
        Neutral,
        Useless
    }
    public static Task AddLogEntry(LogSignificance significance,
        IEnumerable<string> tags,
        string title,
        SocketUser? userSource,
        params (object cause, object effect)[] data) =>
        AddLogEntry(significance, tags, title, userSource, data, null);

    public static Task AddLogEntry(LogSignificance significance,
        IEnumerable<string> tags,
        string title,
        SocketUser? userSource,
        (object cause, object effect)[] data,
        string? imageUrl)
    {
        static string Tagify(string prevStr) => $"[TAG_{prevStr.ToUpper().Replace(' ', '_')}]";

        var builder = new EmbedBuilder()
            .WithColor(significance switch
            {
                LogSignificance.Dangerous => Color.Red,
                LogSignificance.Warning => Color.Gold,
                LogSignificance.Neutral => Color.Blue,
                _ => new Color(128, 128, 128)
            })
            .WithTitle(title);

        string tagPrefix = $"{Tagify(significance.Humanize())} {string.Join(" ", tags.Select(Tagify))}";

        if (userSource is { })
        {
            builder.WithAuthor(userSource)
                .WithDescription($"User: {userSource.Mention} : {userSource.Id}");
            tagPrefix += $" {Tagify($"user={userSource.Id}")}";
        }

        if (data is { Length: > 0 })
            foreach ((object cause, object effect) in data)
                builder.AddField(
                    cause.ToString() is {Length: > 0} causeString 
                        ? causeString 
                        : "`Null`",
                    effect.ToString() is {Length: > 0} effectString 
                        ? effectString 
                        : "`Null`");

        if (imageUrl is { })
            builder.WithImageUrl(imageUrl)
                .AddField("Image URL", $"{imageUrl}");
        
        void Send(ISocketMessageChannel channel) => channel.SendMessageAsync(tagPrefix, embed: builder!.Build());
        
        Send(significance == LogSignificance.Useless
                ? RuntimeConfig.UselessLogs
                : RuntimeConfig.TLCLogs);
        
        return Task.CompletedTask;
    }
}