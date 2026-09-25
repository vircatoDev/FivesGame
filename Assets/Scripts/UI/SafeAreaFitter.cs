using UnityEngine;

namespace Scripts.UI
{
    /// <summary>
    /// Keeps this rect inside <see cref="Screen.safeArea"/>, away from notches, the Dynamic Island and rounded corners.
    /// Put interactive UI inside it; backgrounds stay outside and fill the whole screen.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect _area;
        private Vector2Int _screen;

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != _area || Screen.width != _screen.x || Screen.height != _screen.y)
                Apply();
        }

        private void Apply()
        {
            _area = Screen.safeArea;
            _screen = new Vector2Int(Screen.width, Screen.height);
            if (_screen.x == 0 || _screen.y == 0)
                return;

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(_area.xMin / _screen.x, _area.yMin / _screen.y);
            rect.anchorMax = new Vector2(_area.xMax / _screen.x, _area.yMax / _screen.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
