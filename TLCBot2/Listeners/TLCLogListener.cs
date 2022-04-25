using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Humanizer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
        Program.Client.UserBanned += OnUserBanned;
        Program.Client.UserUnbanned += OnUserUnbanned;
        Program.Client.UserJoined += OnUserJoined;
        Program.Client.UserLeft += OnUserLeft;
        Program.Client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        Program.Client.GuildScheduledEventCancelled += OnScheduledEventCancelled;
        Program.Client.GuildScheduledEventCompleted += OnScheduledEventCompleted;
        Program.Client.GuildScheduledEventCreated += OnScheduledEventCreated;
        Program.Client.GuildScheduledEventStarted += OnScheduledEventStarted;
        Program.Client.GuildScheduledEventUpdated += OnScheduledEventUpdated;
        Program.Client.GuildScheduledEventUserAdd += OnScheduledEventUserAdd;
        Program.Client.GuildScheduledEventUserRemove += OnScheduledEventUserRemove;
        Program.Client.GuildMemberUpdated += OnGuildUserUpdated;
        Program.Client.ThreadMemberJoined += OnThreadMemberJoined;
        Program.Client.ThreadMemberLeft += OnThreadMemberLeft;
        Program.Client.GuildJoinRequestDeleted += OnJoinRequestDeleted;
    }

    public static Task OnJoinRequestDeleted(Cacheable<SocketGuildUser, ulong> userCacheable, SocketGuild guild)
    {
        if (userCacheable.GetOrDownloadAsync().Result is not { } user) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Neutral,
            new[] {"join request deleted"},
            "Newjoin Declined Welcome Screening Board",
            user,
            ("Account Creation Date",
                $"<t:{user.CreatedAt.ToUnixTimeSeconds()}:F> <t:{user.CreatedAt.ToUnixTimeSeconds()}:R>"),
            ("Has Nitro", user.PremiumSince != null));
    }
    public static Task OnThreadMemberLeft(SocketThreadUser user) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"thread member left", "user change", "threads"},
            "Member Left Thread",
            user,
            ("Thread", user.Thread.Mention),
            ("Thread Channel", $"<#{user.Thread.ParentChannel.Id}."));
    public static Task OnThreadMemberJoined(SocketThreadUser user) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"thread member joined", "user change", "threads"},
            "Member Joined Thread",
            user,
            ("Thread", user.Thread.Mention),
            ("Thread Channel", $"<#{user.Thread.ParentChannel.Id}."));
    public static Task OnScheduledEventUserAdd(
        Cacheable<SocketUser, RestUser, IUser, ulong> userCacheable,
        SocketGuildEvent guildEvent)
    {
        if (userCacheable.Value is not { } user) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Useless,
            new[] {"events", "server change", "event user added"},
            "Event User Added",
            user,
            ("Event Name", guildEvent.Name));
    }
    public static Task OnScheduledEventUserRemove(
        Cacheable<SocketUser, RestUser, IUser, ulong> userCacheable,
        SocketGuildEvent guildEvent)
    {
        if (userCacheable.Value is not { } user) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Useless,
            new[] {"events", "server change", "event user removed"},
            "Event User Removed",
            user,
            ("Event Name", guildEvent.Name));
    }
    public static Task OnScheduledEventUpdated(
        Cacheable<SocketGuildEvent, ulong> oldGuildEventCacheable, 
        SocketGuildEvent newGuildEvent)
    {
        var oldGuildEvent = oldGuildEventCacheable.GetOrDownloadAsync().Result;
        if (oldGuildEvent == null) return Task.CompletedTask;
        var oldData = 
               (oldGuildEvent.Name,
                oldGuildEvent.Description,
                oldGuildEvent.StartTime, 
                oldGuildEvent.EndTime,
                oldGuildEvent.UserCount,
                oldGuildEvent.Creator.Mention);
        var newData = 
               (newGuildEvent.Name,
                newGuildEvent.Description,
                newGuildEvent.StartTime, 
                newGuildEvent.EndTime,
                newGuildEvent.UserCount,
                newGuildEvent.Creator.Mention);
        if (newData == oldData) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Neutral,
            new[] {"events", "server change", "server event updated"},
            "Server Event Updated",
            null,
            ("Event Name", oldData.Name),
            ("Event Description", $"{oldData.Description} → {newData.Description}"),
            ("Event Start Time", $"{oldData.StartTime.Humanize()} → {newData.StartTime.Humanize()}"),
            ("Event End Time", $"{oldData.EndTime.Humanize()} → {newData.EndTime.Humanize()}"),
            ("Event Participants", $"{oldData.UserCount ?? 0} → {newData.UserCount ?? 0}"),
            ("Event Organizer", $"{oldData.Mention} → {newData.Mention}"));
    }

    public static Task OnScheduledEventCancelled(SocketGuildEvent guildEvent) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"events", "server change", "server event cancelled"},
            "Server Event Cancelled",
            null,
            ("Event Name", guildEvent.Name),
            ("Event Description", guildEvent.Description),
            ("Event Start Time", guildEvent.StartTime.Humanize()),
            ("Event End Time", guildEvent.EndTime.Humanize()),
            ("Event Participants", guildEvent.UserCount ?? 0),
            ("Event Organizer", guildEvent.Creator.Mention));
    public static Task OnScheduledEventCompleted(SocketGuildEvent guildEvent) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"events", "server change", "server event completed"},
            "Server Event Completed",
            null,
            ("Event Name", guildEvent.Name),
            ("Event Description", guildEvent.Description),
            ("Event Start Time", guildEvent.StartTime.Humanize()),
            ("Event End Time", guildEvent.EndTime.Humanize()),
            ("Event Participants", guildEvent.UserCount ?? 0),
            ("Event Organizer", guildEvent.Creator.Mention));
    public static Task OnScheduledEventCreated(SocketGuildEvent guildEvent) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"events", "server change", "server event created"},
            "Server Event Created",
            null,
            ("Event Name", guildEvent.Name),
            ("Event Description", guildEvent.Description),
            ("Event Start Time", guildEvent.StartTime.Humanize()),
            ("Event End Time", guildEvent.EndTime.Humanize()),
            ("Event Participants", guildEvent.UserCount ?? 0),
            ("Event Organizer", guildEvent.Creator.Mention));
    public static Task OnScheduledEventStarted(SocketGuildEvent guildEvent) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"events", "server change", "server event started"},
            "Server Event Started",
            null,
            ("Event Name", guildEvent.Name),
            ("Event Description", guildEvent.Description),
            ("Event Start Time", guildEvent.StartTime.Humanize()),
            ("Event End Time", guildEvent.EndTime.Humanize()),
            ("Event Participants", guildEvent.UserCount ?? 0),
            ("Event Organizer", guildEvent.Creator.Mention));
    public static Task OnUserVoiceStateUpdated(
        SocketUser user,
        SocketVoiceState oldState,
        SocketVoiceState newState)
    {
        var oldData = (oldState.VoiceChannel, oldState.IsDeafened, oldState.IsMuted);
        var newData = (newState.VoiceChannel, newState.IsDeafened, newState.IsMuted);
        if (oldData == newData) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Useless,
            new[] {"user voice state updated", "user change"},
            "User Voice State Updated",
            user,
            ("Voice Channel", $"{oldData.VoiceChannel.Mention} → {newData.VoiceChannel.Mention}"),
            ("Server Deafened", $"{oldData.IsDeafened} → {newData.IsDeafened}"),
            ("Server Muted", $"{oldData.IsMuted} → {newData.IsMuted}"));
    }
    public static Task OnGuildUserUpdated(Cacheable<SocketGuildUser, ulong> oldUserCacheable, SocketGuildUser newUser)
    {
        if (oldUserCacheable.GetOrDownloadAsync().Result is not { } oldUser) return Task.CompletedTask;
        var oldData = (oldUser.Username, oldUser.Nickname, oldUser.GetAvatarUrl());
        var newData = (newUser.Username, newUser.Nickname, newUser.GetAvatarUrl());
        if (oldData == newData) return Task.CompletedTask;
        return AddLogEntry(LogSignificance.Neutral,
            new[] {"user updated", "user change"},
            "User Updated",
            oldUser,
            new (object, object)[]
            {
                ("Username", $"{oldData.Username} → {newData.Username}"),
                ("Nickname", $"{oldData.Nickname} → {newData.Nickname}"),
                ("Avatar", $"{oldData.Item2} → {newData.Item2}")
            }, newData.Item2);
    }
    public static Task OnUserJoined(SocketGuildUser user) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"user joined", "user change", "server change"},
            "User Joined",
            user,
            ("Account Creation Date", $"<t:{user.CreatedAt.ToUnixTimeSeconds()}:F> <t:{user.CreatedAt.ToUnixTimeSeconds()}:R>"),
            ("Has Nitro", user.PremiumSince != null));
    public static Task OnUserLeft(SocketGuild _, SocketUser user) =>
        AddLogEntry(LogSignificance.Neutral,
            new[] {"user left", "user change", "server change"},
            "User Left",
            user);
    public static Task OnUserBanned(SocketUser user, SocketGuild _) =>
        AddLogEntry(LogSignificance.Warning,
            new[] {"ban", "user banned", "user change", "server change"},
            "User Banned",
            user);
    public static Task OnUserUnbanned(SocketUser user, SocketGuild _) =>
        AddLogEntry(LogSignificance.Warning,
            new[] {"ban", "user unbanned", "user change", "server change"},
            "User Unbanned",
            user);
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
        AddLogEntry(LogSignificance.Useless,
            new[] {"slash command executed", "command", "interaction"},
            "Slash Command Executed",
            command.User,
            ("Command Name", command.CommandName),
            ("Parameters Used",
                command.Data.Options is {Count: > 0}
                    ? string.Join("\n", command.Data.Options.Select(x => $"{x.Name}: {x.Value}"))
                    : "None"));
    public static Task OnModalSubmitted(SocketModal modal) =>
        AddLogEntry(LogSignificance.Useless,
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
        AddLogEntry(LogSignificance.Useless,
            new[] {"selection menu executed", "message component", "interaction"},
            "Selection Menu Executed",
            selectionMenu.User,
            ("Selected Value(s)",
                string.Join("\n", selectionMenu.Data.Values.Select((x, i) => 
                    $"`{i+1}` {x}"))),
            ("Original Message", $"[Go to message]({selectionMenu.Message.GetJumpUrl()})"),
            ("Unique ID", selectionMenu.Data.CustomId));
    public static Task OnButtonExecuted(SocketMessageComponent button) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"button executed", "message component", "interaction"},
            "Button Executed",
            button.User,
            ("Original Message", $"[Go to message]({button.Message.GetJumpUrl()})"),
            ("Unique ID", button.Data.CustomId));
    public static Task OnUserCommandExecuted(SocketUserCommand command) =>
        AddLogEntry(LogSignificance.Useless,
            new[] {"user command executed", "command", "interaction"},
            "User Command Executed", 
            command.User, 
            ("Command Name", command.CommandName),
            ("Used On", command.Data.Member.Mention));
    public static Task OnMessageCommandExecuted(SocketMessageCommand command) => 
        AddLogEntry(LogSignificance.Useless,
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