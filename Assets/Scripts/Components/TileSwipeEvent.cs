namespace Scripts.Components
{
    /// <summary>A swipe from a tile toward a side: Dx is the column step, Dy the row step (rows grow downward).</summary>
    public struct TileSwipeEvent
    {
        public int Id;
        public int Dx;
        public int Dy;
    }
}
