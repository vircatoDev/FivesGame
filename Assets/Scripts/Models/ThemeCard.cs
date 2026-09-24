using UnityEngine;

namespace Scripts.Models
{
    /// <summary>What a main menu carousel card shows for a theme.</summary>
    public readonly struct ThemeCard
    {
        public readonly Sprite Image;
        public readonly string Title;
        public readonly string Progress;

        public ThemeCard(Sprite image, string title, string progress)
        {
            Image = image;
            Title = title;
            Progress = progress;
        }
    }
}
