using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    class WinCheckSystem : IEcsRunSystem
    {
        private const float ResultStateDelay = 2f;

        private readonly EcsFilter<TileComponent> _tileFilter = null;
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;
        private readonly EcsWorld _world;
        private readonly GameSession _gameSession;

        private readonly EcsFilter<TileComponent, MoveComponent> _moveFilter;
        private readonly EcsFilter<GameEndEvent> _endEvents;
        private float _elapsedSinceWin;

        public void Run()
        {
            if (_stateFilter.Get1(0).CurrentState != GameStateType.Playing)
            {
                _elapsedSinceWin = 0f;
                return;
            }

            // A manual exit takes precedence over the delayed result screen.
            if (_endEvents.GetEntitiesCount() > 0)
                return;

            if (!_gameSession.IsCompleted)
            {
                if (_moveFilter.GetEntitiesCount() > 0 || !IsBoardSolved())
                {
                    return;
                }

                _gameSession.CompleteRun();
                SendWinSound();
            }

            if (_elapsedSinceWin >= ResultStateDelay)
                return;

            _elapsedSinceWin += Time.unscaledDeltaTime;

            if (_elapsedSinceWin < ResultStateDelay)
            {
                return;
            }

            _world.NewEntity().Get<GameEndEvent>();
            _world.NewEntity().Replace(new ChangeStateEvent
            {
                NewStateName = GameStateType.Finished
            });
        }

        private bool IsBoardSolved()
        {
            if (_tileFilter.GetEntitiesCount() != BoardMath.CellCount(_gameSession.SelectedGameMode.BoardSize))
                return false;

            foreach (var i in _tileFilter)
            {
                ref var tile = ref _tileFilter.Get1(i);

                var expectedId = BoardMath.TileIdAt(
                    (int)tile.Position.x,
                    (int)tile.Position.y,
                    _gameSession.SelectedGameMode.BoardSize);

                if (tile.Id != expectedId)
                {
                    return false;
                }
            }

            return true;
        }

        private void SendWinSound()
        {
            Debug.Log("Game completed!");

            _world.NewEntity().Replace(new PlaySoundEffectEvent
            {
                Key = AudioKeyCollection.Win,
                Volume = 1f
            });
        }
    }
}
