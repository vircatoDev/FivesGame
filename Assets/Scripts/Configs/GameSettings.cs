using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [FormerlySerializedAs("BoardSize")] public int Columns = 4;
        public int Rows = 3;
        public float TileSize;
        public float TileSpacing;
        public Sprite[] TileSprites;

        /// <summary>Size of the tile grid, without the frame.</summary>
        public Vector2 BoardArea => new Vector2(Columns * TileSize + (Columns - 1) * TileSpacing,
            Rows * TileSize + (Rows - 1) * TileSpacing);

        /// <summary>Anchored position of a board cell: row-major, rows grow downward.</summary>
        public Vector2 CellToAnchored(int cell)
        {
            var step = TileSize + TileSpacing;
            return new Vector2(cell % Columns * step, -(cell / Columns) * step);
        }
    }
}