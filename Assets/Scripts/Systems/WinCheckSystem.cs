using System;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    /// <summary>
    /// Marks the board solved once its last move has finished, records the result, and after a pause
    /// ends the run and opens the result screen.
    /// </summary>
    class WinCheckSystem : IEcsRunSystem
    {
        private const float ResultStateDelay = 2f;

        private readonly EcsWorld _world = null;
        private readonly GameSession _gameSession = null;
        private readonly IFrameTime _time = null;
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;
        private readonly EcsFilter<BoardComponent, BoardHistoryComponent>.Exclude<BoardSolvedTag> _playing = null;
        private readonly EcsFilter<BoardSolvedTag> _solved = null;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves = null;
        private readonly EcsFilter<GameEndEvent> _ends = null;
        private float _elapsedSinceWin;

        public void Run()
        {
            if (_stateFilter.Get1(0).CurrentState != GameStateType.Playing)
            {
                _elapsedSinceWin = 0f;
                return;
            }

            // A manual exit takes precedence over the delayed result screen.
            if (!_ends.IsEmpty())
                return;

            if (_moves.IsEmpty())
            {
                foreach (var i in _playing)
                {
                    if (_playing.Get1(i).State.IsSolved)
                        Complete(_playing.GetEntity(i), _playing.Get2(i));
                }
            }

            if (_solved.IsEmpty() || _elapsedSinceWin >= ResultStateDelay)
                return;

            _elapsedSinceWin += _time.UnscaledDeltaTime;
            if (_elapsedSinceWin < ResultStateDelay)
                return;

            _world.Send<GameEndEvent>();
            _world.ChangeState(GameStateType.Finished);
        }

        private void Complete(EcsEntity board, in BoardHistoryComponent history)
        {
            board.Get<BoardSolvedTag>();
            _gameSession.CompleteRun(history.Moves.Count, TimeSpan.FromSeconds(_time.RealtimeSinceStartup - history.StartTime));
            _world.PlaySound(AudioKeyCollection.Win);
            _world.Send<BoardSolvedEvent>();
        }
    }
}
