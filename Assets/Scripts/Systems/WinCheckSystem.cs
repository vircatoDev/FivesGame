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

        private bool _winDetected;
        private float _elapsedSinceWin;

        public void Run()
        {
            if (_stateFilter.Get1(0).CurrentState != GameStateType.Playing)
            {
                ResetWinSequence();
                return;
            }

            if (!_winDetected)
            {
                if (!IsBoardSolved())
                {
                    return;
                }

                _winDetected = true;
                SendWinSound();
            }

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

        private void ResetWinSequence()
        {
            _winDetected = false;
            _elapsedSinceWin = 0f;
        }

        private bool IsBoardSolved()
        {
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

            return _tileFilter.GetEntitiesCount() > 0;
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
