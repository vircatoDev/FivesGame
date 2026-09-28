using System;
using System.Collections.Generic;
using System.Linq;

namespace Fives.UI
{
    /// <summary>A title for the select screen's plate, which has room for two lines: one on the cloud, one on the plank.</summary>
    public static class TwoLineTitle
    {
        /// <summary>
        /// Breaks the title at the space that makes the two lines closest in length. On a tie the first line is the
        /// shorter one, so an article stays with its noun: "CHOISIS / UN THÈME". A single word stays on one line.
        /// </summary>
        public static string Split(string title)
        {
            var words = title.Replace("<br>", " ").Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2)
                return Line(words);

            var best = 1;
            for (var split = 2; split < words.Length; split++)
                if (Imbalance(words, split) < Imbalance(words, best))
                    best = split;
            return Line(words.Take(best)) + "\n" + Line(words.Skip(best));
        }

        private static int Imbalance(string[] words, int split) =>
            Math.Abs(Line(words.Take(split)).Length - Line(words.Skip(split)).Length);

        private static string Line(IEnumerable<string> words) => string.Join(" ", words);
    }
}
