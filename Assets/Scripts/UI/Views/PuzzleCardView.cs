using Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    /// <summary>One card of the main menu carousel: a theme with the picture of its next puzzle and its progress.</summary>
    public class PuzzleCardView : MonoBehaviour
    {
        [SerializeField] private Image preview;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI progress;

        public RectTransform Rect => (RectTransform)transform;

        public void Show(in ThemeCard card)
        {
            preview.sprite = card.Image;
            title.text = card.Title;
            progress.text = card.Progress;
        }
    }
}
