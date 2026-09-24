using DanielLochner.Assets.SimpleScrollSnap;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.UI
{
    /// <summary>
    /// Scroll-snap transition effect for theme cards. Cards tilt away from the center in proportion to their distance,
    /// only the centered card uses the highlighted frame and draws on top, and cards fade out on the way to the point
    /// where infinite scrolling moves them to the opposite end, so the move is never visible.
    /// </summary>
    [RequireComponent(typeof(SimpleScrollSnap))]
    public class ThemeCardTilt : MonoBehaviour
    {
        [SerializeField] private float maxAngle = 8f;

        private SimpleScrollSnap _scrollSnap;

        private void Awake() => _scrollSnap = GetComponent<SimpleScrollSnap>();

        public void OnTransition(GameObject panel, float displacement)
        {
            var neighbor = _scrollSnap.Size.x * (1f + _scrollSnap.AutomaticLayoutSpacing);
            var wrap = _scrollSnap.Content.rect.width / 2f;
            var t = Mathf.Clamp(displacement / neighbor, -1f, 1f);
            panel.transform.localRotation = Quaternion.Euler(0, 0, -t * maxAngle);
            panel.GetComponent<CanvasGroup>().alpha = 1f - Mathf.InverseLerp(neighbor, wrap, Mathf.Abs(displacement));

            var centered = Mathf.Abs(t) < 0.5f;
            panel.GetComponent<MenuItemView>().SetCentered(centered);
            // Overlapping cards: the centered one draws on top. The scroll snap reads child order only when panels are added.
            if (centered && panel.transform.GetSiblingIndex() != panel.transform.parent.childCount - 1)
                panel.transform.SetAsLastSibling();
        }
    }
}
