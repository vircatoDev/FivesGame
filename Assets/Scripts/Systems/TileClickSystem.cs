using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    public class TileClickSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private readonly EcsFilter<TileClickEvent> _uiClickEvents;
        private readonly EcsFilter<TileComponent> _tileFilter;
        private readonly EcsFilter<TileComponent, EmptyTileComponent> _emptyTileFilter;

        private readonly EcsFilter<GameStateComponent> _stateFilter = null;
        private GameStateComponent _cachedState;

        public void Init()
        {
            foreach (var i in _stateFilter)
            {
                _cachedState = _stateFilter.Get1(i);
                break;
            }
        }

        public void Run()
        {
            foreach (var i in _uiClickEvents)
            {
                ref var clickEvent = ref _uiClickEvents.Get1(i);
                ProcessTileClick(clickEvent.Id);
            }
        }

        private void ProcessTileClick(int tileId)
        {
            foreach (var emptyTileEntity in _emptyTileFilter)
            {
                ref var emptyTile = ref _emptyTileFilter.Get1(emptyTileEntity);

                bool rightTurnDetect = false;

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

                        rightTurnDetect = true;
                        break;
                    }
                    else
                    {
                    }
                }

                if (!rightTurnDetect)
                {
                    var soundEntity = _world.NewEntity();
                    soundEntity.Replace(new PlaySoundEffectEvent()
                    {
                        Key = AudioKeyCollection.WrongClick,
                        Volume = 1f
                    });
                }
            }
        }

        private bool IsAdjacent(Vector3 tilePosition, Vector3 emptyTilePosition)
        {
            var distance = Vector3.Distance(tilePosition, emptyTilePosition);
            return Mathf.Approximately(distance, 1f);
        }
    }
}