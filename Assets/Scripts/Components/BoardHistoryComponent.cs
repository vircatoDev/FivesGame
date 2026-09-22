using System.Collections.Generic;

namespace Scripts.Components
{
    public struct BoardHistoryComponent
    {
        public int Seed;
        public int ShuffleSteps;
        public int InitialEmptyCell;
        public List<int> Moves;
    }
}
