using UnityEngine;

namespace Scripts.Models
{
    [System.Serializable]
    public class PuzzleData
    {
        [Tooltip("Stable id stored in saves, unique across themes; never change it once released.")]
        public string Id;
        public Sprite Image;
        public string Name;
        public string Description;
    }
}