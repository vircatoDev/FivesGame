using System;
using System.Collections.Generic;

namespace Fives.Domain
{
    /// <summary>
    /// A rectangular swap puzzle with every cell filled. Cells and tile IDs are zero-based, in row-major order;
    /// a solved board has tile N in cell N. A move exchanges two orthogonally adjacent cells.
    /// </summary>
    public sealed class BoardState
    {
        private readonly int[] _tiles;
        private readonly int[] _cells;

        public int Columns { get; }
        public int Rows { get; }
        public int CellCount => _tiles.Length;
        public int this[int cell] => _tiles[cell];

        /// <summary>Cell holding the tile, or -1 for an unknown tile ID.</summary>
        public int CellOf(int tileId) => tileId >= 0 && tileId < CellCount ? _cells[tileId] : -1;

        public BoardState Copy() => new BoardState(Columns, Rows, _tiles);

        public bool IsSolved
        {
            get
            {
                for (var cell = 0; cell < CellCount; cell++)
                {
                    if (_tiles[cell] != cell)
                        return false;
                }

                return true;
            }
        }

        /// <summary>Creates a solved board.</summary>
        public BoardState(int columns, int rows)
        {
            var cellCount = BoardMath.CellCount(columns, rows);
            Columns = columns;
            Rows = rows;
            _tiles = new int[cellCount];
            _cells = new int[cellCount];

            for (var cell = 0; cell < cellCount; cell++)
            {
                _tiles[cell] = cell;
                _cells[cell] = cell;
            }
        }

        /// <summary>Copies a permutation of tile IDs.</summary>
        public BoardState(int columns, int rows, IReadOnlyList<int> tiles)
            : this(columns, rows)
        {
            if (tiles == null)
                throw new ArgumentNullException(nameof(tiles));
            if (tiles.Count != CellCount)
                throw new ArgumentException("The tile count must match the board dimensions.", nameof(tiles));

            var seen = new bool[CellCount];
            for (var cell = 0; cell < CellCount; cell++)
            {
                var tile = tiles[cell];
                if (tile < 0 || tile >= CellCount || seen[tile])
                    throw new ArgumentException("Tile IDs must be a permutation of 0 through CellCount - 1.", nameof(tiles));

                seen[tile] = true;
                _tiles[cell] = tile;
                _cells[tile] = cell;
            }
        }

        /// <summary>True for two cells on the board that share a side. Does not change the board.</summary>
        public bool AreNeighbors(int a, int b)
        {
            if (a < 0 || a >= CellCount || b < 0 || b >= CellCount)
                return false;

            var rowDistance = Math.Abs(a / Columns - b / Columns);
            var columnDistance = Math.Abs(a % Columns - b % Columns);
            return rowDistance + columnDistance == 1;
        }

        /// <summary>Exchanges the tiles in two neighboring cells.</summary>
        public bool TrySwap(Swap swap)
        {
            if (!AreNeighbors(swap.First, swap.Second))
                return false;

            var first = _tiles[swap.First];
            var second = _tiles[swap.Second];
            _tiles[swap.First] = second;
            _tiles[swap.Second] = first;
            _cells[first] = swap.Second;
            _cells[second] = swap.First;
            return true;
        }
    }
}
