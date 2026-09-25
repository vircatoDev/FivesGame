using System;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    class WinCheckSystem : IEcsRunSystem
    {
        private const float ResultStateDelay = 2f;

        private readonly EcsFilter<BoardComponent, BoardHistoryComponent> _boards;
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;
        private readonly EcsWorld _world;
        private readonly GameSession _gameSession;
        private readonly IFrameTime _time = null;

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
            if (_endEvents.GetEntitiesCount() > 0 || !_gameSession.IsRunning)
                return;

            if (!_gameSession.IsCompleted)
            {
                if (_moveFilter.GetEntitiesCount() > 0 || _boards.GetEntitiesCount() != 1 || !_boards.Get1(0).State.IsSolved)
                {
                    return;
                }

                ref var history = ref _boards.Get2(0);
                _gameSession.CompleteRun(history.Moves.Count,
                    TimeSpan.FromSeconds(_time.RealtimeSinceStartup - history.StartTime));
                _world.PlaySound(AudioKeyCollection.Win);
                _world.Send<BoardSolvedEvent>();
            }

            if (_elapsedSinceWin >= ResultStateDelay)
                return;

            _elapsedSinceWin += _time.UnscaledDeltaTime;

            if (_elapsedSinceWin < ResultStateDelay)
            {
                return;
            }

            _world.Send<GameEndEvent>();
            _world.ChangeState(GameStateType.Finished);
        }
    }
}
