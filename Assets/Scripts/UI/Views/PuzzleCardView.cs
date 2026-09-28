using Fives.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Fives.UI.Views
{
    /// <summary>One card of the main menu carousel: a theme with the picture of its next puzzle and its progress.</summary>
    public class PuzzleCardView : MonoBehaviour
    {
        [SerializeField] private Image preview;
        [Tooltip("The theme name's label: translates the key it is given and follows a language change.")]
        [SerializeField] private LocalizeStringEvent title;
        [SerializeField] private TextMeshProUGUI progress;

        public RectTransform Rect => (RectTransform)transform;

        public void Show(in ThemeCard card)
        {
            preview.SetCover(card.Image);
            title.SetEntry(card.TitleKey);
            progress.text = card.Progress;
        }
    }
}
