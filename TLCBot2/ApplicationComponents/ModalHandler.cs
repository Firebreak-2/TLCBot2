﻿using Discord;
using Discord.WebSocket;
using TLCBot2.ApplicationComponents.Core;
using TLCBot2.Core;

namespace TLCBot2.ApplicationComponents;

public static class ModalHandler
{
    public static List<FireModal> AllModals = new();
    public static Task OnModalSubmitted(SocketModal modal)
    {
        RuntimeConfig.CommandExecutionLog.SendMessageAsync(embed: new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("Modal Submitted")
            .AddField("Channel Responsible For Modal", modal.Channel)
            .AddField("Parameters Used", 
                modal.Data.Components is not null && modal.Data.Components.Count > 0 
                    ? string.Join("\n", modal.Data.Components.Select(x => $"{x.Value}"))
                    : "None")
            .WithDescription($"User: {modal.User.Mention} : {modal.User.Id}")
            .WithAuthor(modal.User).Build());

        foreach (var fireModal in AllModals)
        {
            if (fireModal.Modal.CustomId != modal.Data.CustomId) continue;
            
            fireModal.OnSubmitted?.Invoke(modal);
            AllModals.Remove(fireModal);
            break;
        }
        return Task.CompletedTask;
    }
}