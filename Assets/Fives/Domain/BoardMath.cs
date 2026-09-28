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
    }
}
