using System;
using System.Collections.Generic;

namespace Fives.Domain
{
    /// <summary>
    /// A square sliding puzzle. Cells and tile IDs are zero-based, in row-major order.
    /// The tile identified by EmptyTileId is hidden; a solved board has tile N in cell N.
    /// </summary>
    public sealed class BoardState
    {
        private readonly int[] _tiles;
        private readonly int[] _cells;

        public int Size { get; }
        public int CellCount => _tiles.Length;
        public int EmptyTileId { get; }
        public int EmptyCell { get; private set; }
        public int this[int cell] => _tiles[cell];

        /// <summary>Cell holding the tile, or -1 for an unknown tile ID.</summary>
        public int CellOf(int tileId) => tileId >= 0 && tileId < CellCount ? _cells[tileId] : -1;

        public BoardState Copy() => new BoardState(Size, EmptyTileId, _tiles);

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

        /// <summary>Creates a solved board with the specified tile hidden.</summary>
        public BoardState(int size, int emptyTileId)
        {
            var cellCount = BoardMath.CellCount(size);
            if (emptyTileId < 0 || emptyTileId >= cellCount)
                throw new ArgumentOutOfRangeException(nameof(emptyTileId));

            Size = size;
            EmptyTileId = emptyTileId;
            EmptyCell = emptyTileId;
            _tiles = new int[cellCount];
            _cells = new int[cellCount];

            for (var cell = 0; cell < cellCount; cell++)
            {
                _tiles[cell] = cell;
                _cells[cell] = cell;
            }
        }

        /// <summary>
        /// Copies a permutation of tile IDs. Validates its structure, not its solvability.
        /// </summary>
        public BoardState(int size, int emptyTileId, IReadOnlyList<int> tiles)
            : this(size, emptyTileId)
        {
            if (tiles == null)
                throw new ArgumentNullException(nameof(tiles));
            if (tiles.Count != CellCount)
                throw new ArgumentException("The tile count must match the board size.", nameof(tiles));

            var seen = new bool[CellCount];
            for (var cell = 0; cell < CellCount; cell++)
            {
                var tile = tiles[cell];
                if (tile < 0 || tile >= CellCount || seen[tile])
                    throw new ArgumentException("Tile IDs must be a permutation of 0 through CellCount - 1.", nameof(tiles));

                seen[tile] = true;
                _tiles[cell] = tile;
                _cells[tile] = cell;
                if (tile == EmptyTileId)
                    EmptyCell = cell;
            }
        }

        /// <summary>Checks a cell index, not a tile ID. Does not change the board.</summary>
        public bool CanMove(int cell)
        {
            if (cell < 0 || cell >= CellCount)
                return false;

            var rowDistance = Math.Abs(cell / Size - EmptyCell / Size);
            var columnDistance = Math.Abs(cell % Size - EmptyCell % Size);
            return rowDistance + columnDistance == 1;
        }

        /// <summary>Slides the tile at the given cell into the empty cell.</summary>
        public bool TryMove(int cell)
        {
            if (!CanMove(cell))
                return false;

            var tile = _tiles[cell];
            _tiles[EmptyCell] = tile;
            _cells[tile] = EmptyCell;
            _tiles[cell] = EmptyTileId;
            _cells[EmptyTileId] = cell;
            EmptyCell = cell;
            return true;
        }
    }
}
