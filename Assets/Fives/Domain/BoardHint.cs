using System;
using System.Collections.Generic;

namespace Fives.Domain
{
    /// <summary>
    /// Hint rules: which misplaced tiles are closest to their cells, and the route one of them should take.
    /// A route is a shortest path, so each swap along it brings the tile one cell closer.
    /// </summary>
    public static class BoardHint
    {
        /// <summary>Misplaced tiles with the smallest distance to their own cell; empty when the board is solved.</summary>
        public static IReadOnlyList<int> ClosestMisplaced(BoardState board)
        {
            var closest = new List<int>();
            var best = int.MaxValue;
            for (var tile = 0; tile < board.CellCount; tile++)
            {
                var distance = Distance(board, board.CellOf(tile), tile);
                if (distance == 0 || distance > best)
                    continue;
                if (distance < best)
                {
                    best = distance;
                    closest.Clear();
                }

                closest.Add(tile);
            }

            return closest;
        }

        /// <summary>
        /// Cells from the tile's current cell to its own cell, both included. Among the shortest paths it takes
        /// the one that moves the fewest correctly placed tiles; ties go horizontal first.
        /// </summary>
        public static IReadOnlyList<int> PathOf(BoardState board, int tileId)
        {
            var start = board.CellOf(tileId);
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(tileId));

            var columns = board.Columns;
            int startColumn = start % columns, startRow = start / columns;
            int stepX = Math.Sign(tileId % columns - startColumn), stepY = Math.Sign(tileId / columns - startRow);
            int width = Math.Abs(tileId % columns - startColumn), height = Math.Abs(tileId / columns - startRow);
            int CellAt(int x, int y) => (startRow + y * stepY) * columns + startColumn + x * stepX;

            // cost[x, y]: fewest placed tiles disturbed from that cell to the target (the start cell is not counted).
            var cost = new int[width + 1, height + 1];
            for (var x = width; x >= 0; x--)
            {
                for (var y = height; y >= 0; y--)
                {
                    var next = int.MaxValue;
                    if (x < width) next = cost[x + 1, y];
                    if (y < height) next = Math.Min(next, cost[x, y + 1]);
                    var cell = CellAt(x, y);
                    cost[x, y] = (next == int.MaxValue ? 0 : next) + (board[cell] == cell ? 1 : 0);
                }
            }

            var path = new List<int> { start };
            for (int x = 0, y = 0; x < width || y < height;)
            {
                if (y == height || (x < width && cost[x + 1, y] <= cost[x, y + 1]))
                    x++;
                else
                    y++;
                path.Add(CellAt(x, y));
            }

            return path;
        }

        private static int Distance(BoardState board, int cell, int target) =>
            Math.Abs(cell % board.Columns - target % board.Columns) + Math.Abs(cell / board.Columns - target / board.Columns);
    }
}
