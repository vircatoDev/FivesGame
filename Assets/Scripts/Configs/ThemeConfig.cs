using Scripts.Models;
using UnityEngine;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game/Theme Config")]
    public class ThemeConfig : ScriptableObject
    {
        [Tooltip("Stable id stored in saves; never change it once released. ThemeName only migrates version-0 saves; the display name is in Localization/Strings.xlsx.")]
        public string Id;
        public string ThemeName;
        public PuzzleData[] Puzzles;
        public Sprite ThemeLogo;
        public int UnlockCost;
    }
}