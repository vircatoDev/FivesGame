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
    }
}