using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Services
{
    /// <summary>Keys of the "UI" string table used from code. Rows live in Localization/Strings.xlsx.</summary>
    public static class TextKeys
    {
        public const string Play = "common.play";
        public const string SelectThemes = "select.themes";
        public const string SelectPuzzles = "select.puzzles";
        public const string Open = "select.open";
        public const string ThemeCompleted = "select.theme_completed";
        public const string PuzzleCompleted = "select.puzzle_completed";
        public const string Moves = "game.moves";
        public const string RewardStars = "result.stars";
        public const string ResultStats = "result.stats";

        public static readonly string[] Fixed =
            { Play, SelectThemes, SelectPuzzles, Open, ThemeCompleted, PuzzleCompleted, Moves, RewardStars, ResultStats };

        public static string Name(ThemeConfig theme) => "theme." + theme.Id;
        public static string Name(PuzzleData puzzle) => "puzzle." + puzzle.Id;
        public static string About(PuzzleData puzzle) => "puzzle." + puzzle.Id + ".about";
    }
}
