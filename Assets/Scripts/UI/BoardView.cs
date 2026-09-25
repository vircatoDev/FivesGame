using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    /// <summary>The board's tile layer and the whole picture that replaces the tiles once the puzzle is solved.</summary>
    public class BoardView : MonoBehaviour
    {
        private const float RevealDuration = 0.6f;

        [SerializeField] private CanvasGroup tiles;
        [SerializeField] private RawImage picture;

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
            tiles.DOFade(0, RevealDuration).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            picture.DOFade(1, RevealDuration).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
