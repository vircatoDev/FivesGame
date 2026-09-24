using System;

namespace Fives.Domain
{
    public static class SeededShuffle
    {
        // Version 2: xorshift32 driving Sattolo's algorithm. The result is a single cycle,
        // so no tile starts in its own cell. Changing this algorithm changes the layout of every seed.
        public static BoardState Create(int size, int seed)
        {
            var tiles = new int[BoardMath.CellCount(size)];
            for (var i = 0; i < tiles.Length; i++)
                tiles[i] = i;

            var random = unchecked((uint)seed);
            if (random == 0)
                random = 0x6D2B79F5;

            for (var i = tiles.Length - 1; i > 0; i--)
            {
                random ^= random << 13;
                random ^= random >> 17;
                random ^= random << 5;
                var j = (int)(random % (uint)i);
                (tiles[i], tiles[j]) = (tiles[j], tiles[i]);
            }

            return new BoardState(size, tiles);
        }
    }
}
