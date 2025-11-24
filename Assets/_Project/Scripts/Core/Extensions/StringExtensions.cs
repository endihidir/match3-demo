using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly char[] SimplifyAllowedChars = { '_', '-' };

        public static string Simplify(this string x)
        {
            return new string(x.ToLower().Trim()
                .Select(c => char.IsLetterOrDigit(c) || SimplifyAllowedChars.Contains(c) ? c : '_')
                .ToArray());
        }

        public static string SplitCamelCase(this string input) => Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();

        public static int ExtractInt(this string s) => int.Parse(Regex.Match(s, @"\d+").Value);

        public static string ConvertToRanking(this int rank)
        {
            string ranking;

            if (rank is > 3 and < 21)
                ranking = "th";
            else
                ranking = (rank % 10) switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th"
                };

            return rank + ranking;
        }

        public static string HideBigNumber(this float num) => num switch
        {
            >= 100000000 => (num / 1000000D).ToString("0.#M"),
            >= 1000000 => (num / 1000000D).ToString("0.##M"),
            >= 100000 => (num / 1000D).ToString("0.#k"),
            >= 10000 => (num / 1000D).ToString("0.##k"),
            >= 1000 => (num / 1000D).ToString("0.#k"),
            _ => num.ToString("0")
        };

        public static int ComputeFnv1AHash(this string val)
        {
            uint hash = 2166136261;

            foreach (var c in val)
            {
                hash = (hash ^ c) * 16777619;
            }

            return unchecked((int)hash);
        }
    }
}