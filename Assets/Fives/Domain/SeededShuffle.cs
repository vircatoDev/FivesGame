using System;

namespace Fives.Domain
{
    public static class SeededShuffle
    {
        // Version 1: xorshift32, left/right/up/down, no immediate reverse move.
        // Changing this algorithm requires a new replay format version.
        public static BoardState Create(int size, int emptyTileId, int seed, int steps)
        {
            if (steps < 1)
                throw new ArgumentOutOfRangeException(nameof(steps));

            var board = new BoardState(size, emptyTileId);
            var random = unchecked((uint)seed);
            if (random == 0)
                random = 0x6D2B79F5;

            var candidates = new int[4];
            var previousCell = -1;
            for (var step = 0; step < steps; step++)
            {
                var count = GetCandidates(board, previousCell, candidates);
                random ^= random << 13;
                random ^= random >> 17;
                random ^= random << 5;
                var cell = candidates[(int)(random % (uint)count)];
                previousCell = board.EmptyCell;
                board.TryMove(cell);
            }

            // A random walk can return to the solved position, particularly on 2x2.
            if (board.IsSolved)
            {
                GetCandidates(board, previousCell, candidates);
                board.TryMove(candidates[0]);
            }

            return board;
        }

        private static int GetCandidates(BoardState board, int excludedCell, int[] candidates)
        {
            var count = 0;
            Add(board.EmptyCell - 1);
            Add(board.EmptyCell + 1);
            Add(board.EmptyCell - board.Size);
            Add(board.EmptyCell + board.Size);
            return count;

            void Add(int cell)
            {
                if (cell != excludedCell && board.CanMove(cell))
                    candidates[count++] = cell;
            }
        }
    }
}
