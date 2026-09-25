using System;

namespace Scripts.Models
{
    /// <summary>What the gameplay HUD shows about the current board. The seed identifies the board, so a new run always refreshes.</summary>
    public readonly struct BoardHud : IEquatable<BoardHud>
    {
        public readonly int Seed;
        public readonly int Moves;
        public readonly bool CanUndo;
        public readonly bool CanHint;
        public readonly int HintPrice;

        public BoardHud(int seed, int moves, bool canUndo, bool canHint, int hintPrice)
        {
            Seed = seed;
            Moves = moves;
            CanUndo = canUndo;
            CanHint = canHint;
            HintPrice = hintPrice;
        }

        public bool Equals(BoardHud other) =>
            Seed == other.Seed && Moves == other.Moves && CanUndo == other.CanUndo && CanHint == other.CanHint && HintPrice == other.HintPrice;
    }
}
