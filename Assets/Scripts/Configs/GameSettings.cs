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

        /// <summary>Size of the tile grid, without the frame.</summary>
        public Vector2 BoardArea => new Vector2(Columns * TileSize + (Columns - 1) * TileSpacing,
            Rows * TileSize + (Rows - 1) * TileSpacing);

        /// <summary>Centre of a board cell in the same space as <see cref="CellToAnchored"/>.</summary>
        public Vector2 CellCenter(int cell) => CellToAnchored(cell) + new Vector2(TileSize, -TileSize) / 2;

        /// <summary>Anchored position of a board cell: row-major, rows grow downward.</summary>
        public Vector2 CellToAnchored(int cell)
        {
            var step = TileSize + TileSpacing;
            return new Vector2(cell % Columns * step, -(cell / Columns) * step);
        }
    }
}