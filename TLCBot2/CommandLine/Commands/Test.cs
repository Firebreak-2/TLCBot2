﻿using Discord;
using Discord.WebSocket;
using Humanizer;
using TLCBot2.Attributes;
using TLCBot2.Core;
using TLCBot2.Data.RuntimeConfig;
using TLCBot2.Data.StringPrompts;
using TLCBot2.Listeners;
using TLCBot2.Types;
using TLCBot2.Utilities;

namespace TLCBot2.CommandLine.Commands;

public static partial class TerminalCommands
{
    [TerminalCommand(Description = "A test command that will likely do something unexpected. Leave this for Firebreak.")]
    public static async Task Test()
    {
        
    }
}