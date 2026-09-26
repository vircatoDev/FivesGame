namespace Scripts.Components
{
    /// <summary>On the board entity from the frame it is solved until the board is destroyed. Gameplay filters exclude it.</summary>
    public struct BoardSolvedComponent
    {
        /// <summary>Unscaled seconds since the solve: the result screen opens when they reach the pause.</summary>
        public float Elapsed;
    }
}
