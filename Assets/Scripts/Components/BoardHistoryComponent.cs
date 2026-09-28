using System.Collections.Generic;
using Fives.Domain;

namespace Fives.Components
{
    public struct BoardHistoryComponent
    {
        public int Seed;
        public List<Swap> Moves;
        public float StartTime;
    }
}
