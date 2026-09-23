using DG.Tweening;
using Leopotam.Ecs;
using Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class TileUiProvider : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler
    {  
        [SerializeField] private CanvasGroup _tileCanvas;
        [SerializeField] private RawImage _tileImage;
        [SerializeField] private int _id;
    
        private const float SelectedScale = 1.08f;

        private EcsWorld _world;
    
        float delayBetweenTiles = 0.1f; // Delay between tiles appearing
        float animationDuration = 0.3f; // Duration of each tile animation

        /// <summary>Shows the uvRect part of the shared puzzle texture; the tile owns no graphics resources.</summary>
        public void Init(EcsWorld world, int id, Texture texture, Rect uvRect)
        {
            _world = world;
            _id = id;
            _tileImage.texture = texture;
            _tileImage.uvRect = uvRect;
            _tileImage.color = Color.white;
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
            var currentDelay = _id * delayBetweenTiles;
            transform
                .DOScale(Vector3.one, animationDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(currentDelay)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            _tileCanvas
                .DOFade(1, animationDuration)
                .SetDelay(currentDelay)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    }
}
