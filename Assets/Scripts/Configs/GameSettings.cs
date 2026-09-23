using UnityEngine;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game Settings")]
    public class GameSettings : ScriptableObject
    {
        public int BoardSize = 3;
        public float TileSize;
        public float TileSpacing;
        public Sprite[] TileSprites;

        /// <summary>Anchored position of a board cell: row-major, rows grow downward.</summary>
        public Vector2 CellToAnchored(int cell)
        {
            var step = TileSize + TileSpacing;
            return new Vector2(cell % BoardSize * step, -(cell / BoardSize) * step);
        }
    }
}