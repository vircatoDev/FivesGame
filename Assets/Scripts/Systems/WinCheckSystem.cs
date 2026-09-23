using System;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    class WinCheckSystem : IEcsRunSystem
    {
        private const float ResultStateDelay = 2f;

        private readonly EcsFilter<BoardComponent, BoardHistoryComponent> _boards;
        private readonly EcsFilter<BoardReplayComponent> _replays;
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
            if (_endEvents.GetEntitiesCount() > 0 || !_gameSession.IsRunning || _replays.GetEntitiesCount() > 0)
                return;

            if (!_gameSession.IsCompleted)
            {
                if (_moveFilter.GetEntitiesCount() > 0 || _boards.GetEntitiesCount() != 1 || !_boards.Get1(0).State.IsSolved)
                {
                    return;
                }

                ref var history = ref _boards.Get2(0);
                _gameSession.CompleteRun(history.Moves.Count,
                    TimeSpan.FromSeconds(Time.realtimeSinceStartup - history.StartTime));
                _world.PlaySound(AudioKeyCollection.Win);
            }

            if (_elapsedSinceWin >= ResultStateDelay)
                return;

            _elapsedSinceWin += Time.unscaledDeltaTime;

            if (_elapsedSinceWin < ResultStateDelay)
            {
                return;
            }

            _world.Send<GameEndEvent>();
            _world.ChangeState(GameStateType.Finished);
        }
    }
}
