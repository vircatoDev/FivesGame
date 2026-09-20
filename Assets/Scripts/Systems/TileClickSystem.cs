using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    public class TileClickSystem : IEcsRunSystem
    {
        private EcsWorld _world;
        private readonly EcsFilter<TileClickEvent> _uiClickEvents;
        private readonly EcsFilter<TileComponent> _tileFilter;
        private readonly EcsFilter<TileComponent, EmptyTileComponent> _emptyTileFilter;
        private readonly EcsFilter<TileComponent, MoveComponent> _moveFilter;

        public void Run()
        {
            if (_moveFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            foreach (var i in _uiClickEvents)
            {
                ref var clickEvent = ref _uiClickEvents.Get1(i);
                ProcessTileClick(clickEvent.Id);
                return;
            }
        }

        private void ProcessTileClick(int tileId)
        {
            foreach (var emptyTileEntity in _emptyTileFilter)
            {
                ref var emptyTile = ref _emptyTileFilter.Get1(emptyTileEntity);

                var validMove = false;

                foreach (var tileEntity in _tileFilter)
                {
                    ref var tile = ref _tileFilter.Get1(tileEntity);

                    if (tile.Id == tileId && IsAdjacent(tile.Position, emptyTile.Position))
                    {
                        var moveEntity = _tileFilter.GetEntity(tileEntity);
                        moveEntity.Replace(new MoveComponent
                        {
                            Direction = (emptyTile.Position - tile.Position).normalized,
                            Speed = 0.35f,
                            TargetPosition = emptyTile.Position
                        });

                        var soundEntity = _world.NewEntity();
                        soundEntity.Replace(new PlaySoundEffectEvent()
                        {
                            Key = AudioKeyCollection.RightTap,
                            Volume = 1f
                        });

                        validMove = true;
                        break;
                    }
                }

                if (!validMove)
                {
                    var soundEntity = _world.NewEntity();
                    soundEntity.Replace(new PlaySoundEffectEvent()
                    {
                        Key = AudioKeyCollection.WrongClick,
                        Volume = 1f
                    });
                }

                return;
            }
        }

        private bool IsAdjacent(Vector3 tilePosition, Vector3 emptyTilePosition)
        {
            var distance = Vector3.Distance(tilePosition, emptyTilePosition);
            return Mathf.Approximately(distance, 1f);
        }
    }
}
