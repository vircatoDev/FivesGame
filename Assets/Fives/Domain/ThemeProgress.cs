using System.Collections.Generic;
using System.Linq;

namespace Fives.Domain
{
    public readonly struct ThemeProgress
    {
        public int Completed { get; }
        public int Total { get; }
        public bool IsComplete => Completed == Total;

        public ThemeProgress(IReadOnlyCollection<string> puzzles, IEnumerable<string> completedPuzzles)
        {
            Completed = puzzles.Intersect(completedPuzzles).Count();
            Total = puzzles.Count;
        }

        public override string ToString() => $"{Completed}/{Total}";
    }
}
