using System;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    /// <summary>
    /// Marks the board solved once its last move has finished and records the result; after a pause, ends the run and
    /// opens the result screen. Part of the gameplay group, so it only runs in the Playing state.
    /// </summary>
    sealed class WinCheckSystem : IEcsRunSystem
    {
        private const float ResultStateDelay = 2f;

        private readonly EcsWorld _world = null;
        private readonly GameSession _gameSession = null;
        private readonly IFrameTime _time = null;
        private readonly EcsFilter<BoardComponent, BoardHistoryComponent>.Exclude<BoardSolvedComponent> _playing = null;
        private readonly EcsFilter<BoardSolvedComponent> _solved = null;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves = null;
        private readonly EcsFilter<GameEndEvent> _ends = null;

        public void Run()
        {
            // A manual exit takes precedence over the delayed result screen.
            if (!_ends.IsEmpty())
                return;

            if (_moves.IsEmpty())
            {
                foreach (var i in _playing)
                {
                    if (_playing.Get1(i).State.IsSolved)
                        Solve(_playing.GetEntity(i), _playing.Get2(i));
                }
            }

            foreach (var i in _solved)
            {
                ref var solved = ref _solved.Get1(i);
                if (solved.Elapsed >= ResultStateDelay)
                    continue; // already announced; the board goes with this frame's GameEndEvent

                solved.Elapsed += _time.UnscaledDeltaTime;
                if (solved.Elapsed < ResultStateDelay)
                    continue;

                _world.Send<GameEndEvent>();
                _world.ChangeState(GameStateType.Finished);
            }
        }

        private void Solve(EcsEntity board, in BoardHistoryComponent history)
        {
            board.Get<BoardSolvedComponent>();
            _gameSession.CompleteRun(history.Moves.Count, TimeSpan.FromSeconds(_time.RealtimeSinceStartup - history.StartTime));
            _world.PlaySound(AudioKeyCollection.Win);
            _world.Send<BoardSolvedEvent>();
        }
    }
}
