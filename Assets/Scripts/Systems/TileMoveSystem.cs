using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    class TileMoveSystem : IEcsRunSystem
    {
        private readonly EcsFilter<TileComponent, MoveComponent> _moveFilter = null;
        private readonly GameSession _gameSettings;
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;

        public void Run()
        {
            if (_stateFilter.Get1(0).CurrentState != GameStateType.Playing)
                return;

            foreach (var i in _moveFilter)
            {
                ref var tile = ref _moveFilter.Get1(i);
                ref var move = ref _moveFilter.Get2(i);
                var target = CalculateUIPosition(move.TargetPosition);

                if (move.InstaMove)
                {
                    tile.Rect.anchoredPosition = target;
                    _moveFilter.GetEntity(i).Del<MoveComponent>();
                    continue;
                }

                if (!move.Started)
                {
                    move.StartPosition = tile.Rect.anchoredPosition;
                    move.Started = true;
                }

                move.ElapsedTime += Time.deltaTime;
                var progress = move.Duration > 0f ? move.ElapsedTime / move.Duration : 1f;
                tile.Rect.anchoredPosition = Vector2.Lerp(move.StartPosition, target, progress);

                if (progress >= 1f)
                    _moveFilter.GetEntity(i).Del<MoveComponent>();
            }
        }

        private Vector2 CalculateUIPosition(Vector2 position)
        {
            var spacing = _gameSettings.SelectedGameMode.TileSize + _gameSettings.SelectedGameMode.TileSpacing;
            return new Vector2(position.x * spacing, -position.y * spacing);
        }
    }
}
