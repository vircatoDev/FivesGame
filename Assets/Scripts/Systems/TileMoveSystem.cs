using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    class TileMoveSystem : IEcsRunSystem
    {
        private EcsWorld _world;
        private readonly EcsFilter<TileComponent, MoveComponent> _moveFilter = null;
        private readonly EcsFilter<TileComponent, EmptyTileComponent> _emptyTileFilter = null;
        private readonly GameSession _gameSettings;
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;

        private Vector2 _startPositionOnBoard;

        public void Run()
        {
            if (_stateFilter.Get1(0).CurrentState != GameStateType.Playing)
            {
                return;
            }

            foreach (var i in _moveFilter)
            {
                ref var moveTileEntity = ref _moveFilter.GetEntity(i);
                ref var tileComponent = ref _moveFilter.Get1(i);
                ref var moveComponent = ref _moveFilter.Get2(i);

                if (_emptyTileFilter.GetEntitiesCount() != 1)
                {
                    return;
                }

                if (moveComponent.InstaMove)
                {
                    MoveTileWithoutAnimation(tileComponent, moveComponent.TargetPosition);
                }
                else
                {
                    var emptyTileEntity = _emptyTileFilter.GetEntity(0);
                    ref var emptyTileComponent = ref _emptyTileFilter.Get1(0);
                    emptyTileComponent.Position = tileComponent.Position;
                    tileComponent.Position = moveComponent.TargetPosition;
                    emptyTileEntity.Get<EmptyTileComponent>();
                    moveTileEntity.Del<EmptyTileComponent>();

                    AnimateTileMovement(tileComponent, moveComponent.TargetPosition, moveComponent.Speed).Forget();
                }

                moveTileEntity.Del<MoveComponent>();
            }
        }

        private void MoveTileWithoutAnimation(TileComponent tileComponent, Vector3 targetPosition)
        {
            var rectTransform = tileComponent.Rect;
            Vector2 targetUIPosition = CalculateUIPosition(targetPosition);
            rectTransform.anchoredPosition = targetUIPosition;
        }

        private async UniTask AnimateTileMovement(TileComponent tileComponent, Vector3 targetPosition, float speed)
        {
            var rectTransform = tileComponent.Rect;
            Vector2 targetUIPosition = CalculateUIPosition(targetPosition);

            float duration = speed;
            Vector2 startPosition = rectTransform.anchoredPosition;
            float time = 0f;

            while (time < duration)
            {
                if (rectTransform == null)
                    break;

                time += Time.deltaTime;
                float t = time / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetUIPosition, t);
                await UniTask.Yield();
            }

            if (rectTransform != null)
                rectTransform.anchoredPosition = targetUIPosition;
        }

        private Vector2 CalculateUIPosition(Vector2 gridPosition)
        {
            return _startPositionOnBoard + new Vector2(
                gridPosition.x * (_gameSettings.SelectedGameMode.TileSize + _gameSettings.SelectedGameMode.TileSpacing),
                -gridPosition.y * (_gameSettings.SelectedGameMode.TileSize + _gameSettings.SelectedGameMode.TileSpacing)
            );
        }
    }
}