using DG.Tweening;
using Fives.Components;
using Leopotam.Ecs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fives.UI
{
    public class TileUiProvider : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler
    {
        private const float SelectedScale = 1.08f;
        private const float DelayBetweenTiles = 0.1f;
        private const float AppearDuration = 0.3f;

        [SerializeField] private CanvasGroup tileCanvas;
        [SerializeField] private RawImage tileImage;

        private EcsWorld _world;
        private int _id;

        /// <summary>Shows the uvRect part of the shared puzzle texture; the tile owns no graphics resources.</summary>
        public void Init(EcsWorld world, int id, Texture texture, Rect uvRect)
        {
            _world = world;
            _id = id;
            tileImage.texture = texture;
            tileImage.uvRect = uvRect;
            tileImage.color = Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _world.Send(new TileClickEvent { Id = _id });
        }

        // Required for the EventSystem to start a drag; the swipe is read when it ends.
        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var delta = eventData.position - eventData.pressPosition;
            var horizontal = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);
            _world.Send(new TileSwipeEvent
            {
                Id = _id,
                Dx = horizontal ? (int)Mathf.Sign(delta.x) : 0,
                Dy = horizontal ? 0 : -(int)Mathf.Sign(delta.y)
            });
        }

        public void SetSelected(bool selected)
        {
            if (selected)
                transform.SetAsLastSibling();

            transform
                .DOScale(selected ? SelectedScale : 1f, 0.15f)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void PlayTileShowAnimation()
        {
            var currentDelay = _id * DelayBetweenTiles;
            transform
                .DOScale(Vector3.one, AppearDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(currentDelay)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            tileCanvas
                .DOFade(1, AppearDuration)
                .SetDelay(currentDelay)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
