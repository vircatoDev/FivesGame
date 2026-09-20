using Scripts.Models;
using UnityEngine;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game/Theme Config")]
    public class ThemeConfig : ScriptableObject
    {
        public string ThemeName;
        public PuzzleData[] Puzzles;
        public Sprite ThemeLogo;
        public int UnlockCost;
    }
}