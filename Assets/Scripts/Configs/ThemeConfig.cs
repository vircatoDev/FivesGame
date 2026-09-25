using Scripts.Models;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game/Theme Config")]
    public class ThemeConfig : ScriptableObject
    {
        [Tooltip("Stable id stored in saves; never change it once released. ThemeName only migrates version-0 saves; the display name is in Localization/Strings.xlsx.")]
        public string Id;
        public string ThemeName;
        public PuzzleData[] Puzzles;
        [Tooltip("Small picture for the menus, in the ThemePreviews group so menus do not load the theme bundle.")]
        public AssetReferenceSprite Preview;
        public int UnlockCost;
    }
}