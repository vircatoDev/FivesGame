using System;
using System.Collections.Generic;

namespace Fives.Domain
{
    /// <summary>Versioned replay data, independent of Unity and ECS.</summary>
    public sealed class ReplayData
    {
        public const int CurrentVersion = 1;

        public int Version { get; }
        public int Size { get; }
        public int EmptyTileId { get; }
        public int Seed { get; }
        public int ShuffleSteps { get; }
        public IReadOnlyList<int> Moves { get; }

        public ReplayData(int version, int size, int emptyTileId, int seed, int shuffleSteps,
            IReadOnlyList<int> moves)
        {
            if (version != CurrentVersion)
                throw new ArgumentOutOfRangeException(nameof(version), "Unsupported replay version.");
            if (moves == null)
                throw new ArgumentNullException(nameof(moves));

            var board = SeededShuffle.Create(size, emptyTileId, seed, shuffleSteps);
            var copy = new int[moves.Count];
            for (var i = 0; i < moves.Count; i++)
            {
                if (board.IsSolved || !board.TryMove(moves[i]))
                    throw new ArgumentException($"Invalid replay move at index {i}.", nameof(moves));
                copy[i] = moves[i];
            }

            Version = version;
            Size = size;
            EmptyTileId = emptyTileId;
            Seed = seed;
            ShuffleSteps = shuffleSteps;
            Moves = Array.AsReadOnly(copy);
        }
    }
}
