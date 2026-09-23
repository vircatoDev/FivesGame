using System;

namespace Scripts.Models
{
    /// <summary>What the gameplay HUD shows about the current board.</summary>
    public readonly struct BoardHud : IEquatable<BoardHud>
    {
        public readonly int Seed;
        public readonly int Moves;
        public readonly int ReplayPosition;
        public readonly bool Replaying;
        public readonly bool CanUndo;
        public readonly bool CanReplay;

        public BoardHud(int seed, int moves, int replayPosition, bool replaying, bool canUndo, bool canReplay)
        {
            Seed = seed;
            Moves = moves;
            ReplayPosition = replayPosition;
            Replaying = replaying;
            CanUndo = canUndo;
            CanReplay = canReplay;
        }

        public bool Equals(BoardHud other) =>
            Seed == other.Seed && Moves == other.Moves && ReplayPosition == other.ReplayPosition
            && Replaying == other.Replaying && CanUndo == other.CanUndo && CanReplay == other.CanReplay;
    }
}
