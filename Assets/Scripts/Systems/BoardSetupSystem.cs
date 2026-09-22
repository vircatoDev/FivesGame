using System.Collections.Generic;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    public class BoardSetupSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly GameSession _session;
        private readonly EcsFilter<BoardComponent> _boards;
        private readonly EcsFilter<GameStateComponent> _states;
        private readonly EcsFilter<GameEndEvent> _ends;

        public void Run()
        {
            if (!_session.IsRunning || _states.Get1(0).CurrentState != GameStateType.Playing
                || _ends.GetEntitiesCount() > 0 || _boards.GetEntitiesCount() > 0)
                return;

            var size = _session.SelectedGameMode.BoardSize;
            var count = BoardMath.CellCount(size);
            var seed = Random.Range(1, int.MaxValue);
            var steps = count * 4;
            var board = SeededShuffle.Create(size, seed % count, seed, steps);
            var entity = _world.NewEntity();
            entity.Replace(new BoardComponent { State = board });
            entity.Replace(new BoardHistoryComponent
            {
                Seed = seed,
                ShuffleSteps = steps,
                InitialEmptyCell = board.EmptyCell,
                Moves = new List<int>()
            });
        }
    }
}
