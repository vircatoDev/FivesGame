using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    class TileMoveSystem : IEcsRunSystem
    {
        private readonly EcsFilter<TileComponent, MoveComponent> _moveFilter = null;
        private readonly GameSession _gameSettings;
        private readonly IFrameTime _time = null;

        public void Run()
        {
            foreach (var i in _moveFilter)
            {
                ref var tile = ref _moveFilter.Get1(i);
                ref var move = ref _moveFilter.Get2(i);
                var target = _gameSettings.SelectedGameMode.CellToAnchored(move.TargetCell);

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

                move.ElapsedTime += _time.DeltaTime;
                var progress = move.Duration > 0f ? move.ElapsedTime / move.Duration : 1f;
                tile.Rect.anchoredPosition = Vector2.Lerp(move.StartPosition, target, progress);

                if (progress >= 1f)
                    _moveFilter.GetEntity(i).Del<MoveComponent>();
            }
        }
    }
}
