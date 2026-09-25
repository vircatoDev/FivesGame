using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    /// <summary>
    /// The board's tile layer, the hint route over it, and the whole picture that replaces the tiles once the puzzle is solved.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        private const float RevealDuration = 0.6f;
        private const float TargetPulseScale = 1.08f;
        private const float TargetPulseDuration = 0.5f;

        [SerializeField] private CanvasGroup tiles;
        [SerializeField] private RawImage picture;

        [Header("Hint")]
        [SerializeField] private GameObject hint;
        [SerializeField] private RectTransform source;
        [SerializeField] private RectTransform target;
        [SerializeField] private RectTransform arrow;
        [SerializeField] private RectTransform dotTemplate;

        private readonly List<RectTransform> _dots = new List<RectTransform>();
        private Tween _pulse;

        public Transform Tiles => tiles.transform;

        /// <summary>The whole picture over the tile grid, hidden until <see cref="Reveal"/>.</summary>
        public void SetPicture(Texture texture, Rect uvRect, Vector2 size)
        {
            picture.texture = texture;
            picture.uvRect = uvRect;
            picture.rectTransform.sizeDelta = size;
            picture.color = new Color(1, 1, 1, 0);
        }

        public void Reveal()
        {
            HideHint();
            tiles.DOFade(0, RevealDuration).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            picture.DOFade(1, RevealDuration).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        /// <summary>
        /// Route of cell centres from the guided tile to its home: a frame on the tile, dots along the way,
        /// an arrow on the last step and a pulsing frame on the home cell.
        /// </summary>
        public void ShowHint(IReadOnlyList<Vector2> route)
        {
            hint.SetActive(true);
            source.anchoredPosition = route[0];
            target.anchoredPosition = route[route.Count - 1];

            var dots = 0;
            for (var step = 1; step < route.Count - 1; step++)
            {
                Dot(dots++).anchoredPosition = (route[step - 1] + route[step]) / 2;
                Dot(dots++).anchoredPosition = route[step];
            }

            for (var i = 0; i < _dots.Count; i++)
                _dots[i].gameObject.SetActive(i < dots);

            var from = route[route.Count - 2];
            var to = route[route.Count - 1];
            arrow.anchoredPosition = (from + to) / 2;
            arrow.localEulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, to - from));

            _pulse ??= target.DOScale(TargetPulseScale, TargetPulseDuration).SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void HideHint()
        {
            hint.SetActive(false);
            _pulse?.Kill();
            _pulse = null;
            target.localScale = Vector3.one;
        }

        private RectTransform Dot(int index)
        {
            if (index == _dots.Count)
                _dots.Add(Instantiate(dotTemplate, dotTemplate.parent));
            return _dots[index];
        }
    }
}
