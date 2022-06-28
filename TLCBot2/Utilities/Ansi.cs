using System.Text;

namespace TLCBot2.Utilities;

public static partial class Helper
{
    public static class Ansi
    {
        public const string Reset = "\x1B[0m";

        public static string Generate(
            Foreground? foregroundColor = null,
            Background? backgroundColor = null,
            Modifier? textModifier = null)
        {
            StringBuilder color = new();

            if (backgroundColor is not null)
                color.Append($";{(int) backgroundColor}");

            if (foregroundColor is not null)
                color.Append($";{(int) foregroundColor}");

            string mod;
            switch (textModifier)
            {
                case null:
                    mod = "0";
                    break;
                case Modifier.BoldAndUnderline:
                    return $"\x1b[1;0m\x1b[4{color}m";
                default:
                    mod = $"{(int) textModifier}";
                    break;
            }

            if (color.Length == 0)
                color.Append(";1");

            return $"\x1b[{mod}{color}m";
        }

        public enum Modifier
        {
            Bold = 1,
            Underline = 4,
            BoldAndUnderline
        }

        public enum Foreground
        {
            Gray = 30,
            Red = 31,
            Green = 32,
            Yellow = 33,
            Blue = 34,
            Pink = 35,
            Cyan = 36,
            White = 37,
        }

        public enum Background
        {
            FireflyDarkBlue = 40,
            Orange = 41,
            MarbleBlue = 42,
            GreyishTurquoise = 43,
            Gray = 44,
            Indigo = 45,
            LightGray = 46,
            White = 47,
        }

    }
}

public static class AnsiExtensions
{
    public static string Get(this Helper.Ansi.Foreground fg)
    {
        return Helper.Ansi.Generate(fg);
    }
    public static string Get(this Helper.Ansi.Background bg)
    {
        return Helper.Ansi.Generate(null, bg);
    }
    public static string Get(this Helper.Ansi.Modifier mod)
    {
        return Helper.Ansi.Generate(null, null, mod);
    }
}