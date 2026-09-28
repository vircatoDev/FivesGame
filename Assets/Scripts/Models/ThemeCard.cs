using UnityEngine;

namespace Fives.Models
{
    /// <summary>What a main menu carousel card shows for a theme.</summary>
    public readonly struct ThemeCard
    {
        public readonly Sprite Image;
        /// <summary>The theme's name in the UI table; the card's label translates it and follows a language change.</summary>
        public readonly string TitleKey;
        public readonly string Progress;

        public ThemeCard(Sprite image, string titleKey, string progress)
        {
            Image = image;
            TitleKey = titleKey;
            Progress = progress;
        }
    }
}
