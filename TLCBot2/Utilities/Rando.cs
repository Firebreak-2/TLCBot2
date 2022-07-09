using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;

namespace TLCBot2.Utilities;

public static partial class Helper
{
    public static class Rando
    {
        public static string Paragraph(int count = 1)
        {
            var randomizer = RandomizerFactory.GetRandomizer(new FieldOptionsTextLipsum
            {
                Paragraphs = count
            });
            return randomizer.Generate()!;
        }

        public static string Id(string name) => $"{name}-{Int(int.MinValue, int.MaxValue - 1)}";
        public static int Int(int min, int max) => _rand.Next(min, max + 1);
        public static bool Bool() => Int(0, 1) == 0;
    }
}