using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Discord;
using Discord.WebSocket;
using TLC_Beta.Utilities;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace TLC_Beta.Core.CommandLine;

public static class TlcAllCommands
{
    public static void Initialize()
    {
        AddCommand(new TlcCommand("reinitcmds", _ =>
        {
            TlcConsole.ListCommand.Clear();
            Initialize();
            TlcConsole.Print("reinitialization complete");
        }, description:"Reinitializes all commands for real-time debugging"));
        AddCommand(new TlcCommand("help", _ =>
        {
            TlcConsole.Print(string.Join("\n", TlcConsole.ListCommand.Where(x => x.Name != "help")
                .Select(x => $"{x.Name}:\n  Args: {x.Args}\n  Description: {x.Description}\n")));
        }));
        AddCommand(new TlcCommand("test", _ =>
        {
            var bmp = new Bitmap(100, 100);
            bmp.FillColor(Color.Red);
            TlcConsole.Print("attempting to send file");
            // Constants.Channels.Lares.TLCBetaCommandLine
            //     .SendFileAsync(new FileAttachment($"{Program.FileAssetsPath}\\TLC_Logo.png"), "test");
            Constants.Channels.Lares.TLCBetaCommandLine
                .SendMessageAsync(Helper.GetFileUrl(bmp.ToStream(ImageFormat.Png),
                    Constants.Channels.Lares.TLCBetaCommandLine));
            TlcConsole.Print("end");
        }));
        AddCommand(new TlcCommand("kill", _ =>
        {
            TlcConsole.Print("goodbye world");
            Program.Client.LogoutAsync();
            Process.GetCurrentProcess().Kill();
        }));
        AddCommand(new TlcCommand("calc", args =>
        {
            TlcConsole.Print($">>> {Helper.Compute(args[0])}");
        }, 1, "Parses the given input through a calculator and returns the output"));
        AddCommand(new TlcCommand("echo", args =>
        {
            TlcConsole.Print($"{args[0]}");
        }, 1, "Repeats the input"));
    }

    public static void AddCommand(TlcCommand command) => TlcConsole.ListCommand.Add(command);
}