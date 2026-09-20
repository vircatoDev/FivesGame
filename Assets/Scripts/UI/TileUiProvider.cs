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
        [SerializeField] private Image _tileImage;
        [SerializeField] private int _id;
    
        private EcsWorld _world;
    
        float delayBetweenTiles = 0.1f; // Delay between tiles appearing
        float animationDuration = 0.3f; // Duration of each tile animation

        public void Init(EcsWorld world, int id, Sprite tileSprite)
        {
            _world = world;
            _id = id;
            _tileImage.sprite = tileSprite;
            _tileImage.color= Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"OnPointerClick:{_id}");
            var clickEvent = new TileClickEvent() { Id = _id, Sender = gameObject };
            _world.NewEntity().Replace(clickEvent);
        }
    
    
        public void PlayTileShowAnimation()
        {
            var currentDelay = _id * delayBetweenTiles;
            transform
                .DOScale(Vector3.one, animationDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(currentDelay);

            _tileCanvas
                .DOFade(1, animationDuration)
                .SetDelay(currentDelay);
        }
    }
}