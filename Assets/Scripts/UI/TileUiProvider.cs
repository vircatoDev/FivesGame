using DG.Tweening;
using Leopotam.Ecs;
using Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class TileUiProvider : MonoBehaviour, IPointerClickHandler
    {  
        [SerializeField] private CanvasGroup _tileCanvas;
        [SerializeField] private RawImage _tileImage;
        [SerializeField] private int _id;
    
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
