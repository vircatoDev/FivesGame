using System;

namespace Fives.Domain
{
    public static class BoardMath
    {
        public static int CellCount(int boardSize)
        {
            if (boardSize < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(boardSize));
            }

            return checked(boardSize * boardSize);
        }

        public static int TileIdAt(int column, int row, int boardSize)
        {
            if (boardSize < 2 || column < 0 || column >= boardSize || row < 0 || row >= boardSize)
            {
                throw new ArgumentOutOfRangeException();
            }

            return checked(row * boardSize + column);
        }
    }
}
