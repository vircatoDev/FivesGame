using Scripts.Models;
using UnityEngine;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game/Theme Config")]
    public class ThemeConfig : ScriptableObject
    {
        [Tooltip("Stable id stored in saves; never change it once released. ThemeName is display text only.")]
        public string Id;
        public string ThemeName;
        public PuzzleData[] Puzzles;
        public Sprite ThemeLogo;
        public int UnlockCost;
    }
}