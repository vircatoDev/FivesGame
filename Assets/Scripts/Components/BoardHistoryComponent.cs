using System.Collections.Generic;
using Fives.Domain;

namespace Scripts.Components
{
    public struct BoardHistoryComponent
    {
        public int Seed;
        public List<Swap> Moves;
        /// <summary>Undone swaps, most recent last; Redo re-applies them, a new move clears them.</summary>
        public List<Swap> Undone;
        public float StartTime;
    }
}
