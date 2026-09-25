using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.Models
{
    [System.Serializable]
    public class PuzzleData
    {
        [Tooltip("Stable id stored in saves, unique across themes; never change it once released.")]
        public string Id;
        [Tooltip("Full-size picture in its theme's Addressables group.")]
        public AssetReferenceSprite Image;
        [Tooltip("Name stored by version-0 saves, used only to migrate them. Display texts are in Localization/Strings.xlsx.")]
        public string Name;
    }
}