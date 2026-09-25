using System;

namespace Fives.Domain
{
    public static class BoardMath
    {
        public static int CellCount(int columns, int rows)
        {
            if (columns < 2 || rows < 2)
            {
                throw new ArgumentOutOfRangeException(columns < 2 ? nameof(columns) : nameof(rows));
            }

            return checked(columns * rows);
        }

        public static int TileIdAt(int column, int row, int columns, int rows)
        {
            if (columns < 2 || rows < 2 || column < 0 || column >= columns || row < 0 || row >= rows)
            {
                throw new ArgumentOutOfRangeException();
            }

            return checked(row * columns + column);
        }
    }
}
