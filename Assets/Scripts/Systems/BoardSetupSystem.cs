using System.Collections.Generic;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    /// <summary>Turns a start request into the board entity. The board entity is the run: it lives until GameEndEvent.</summary>
    public class BoardSetupSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly GameSession _session = null;
        private readonly IFrameTime _time = null;
        private readonly EcsFilter<StartRunRequest> _requests = null;
        private readonly EcsFilter<BoardComponent> _boards = null;
        private readonly EcsFilter<GameEndEvent> _ends = null;

        public void Run()
        {
            if (_requests.IsEmpty() || !_ends.IsEmpty())
                return;

            foreach (var i in _requests)
                _requests.GetEntity(i).Destroy();

            if (!_boards.IsEmpty())
                return;

            var mode = _session.SelectedGameMode;
            var seed = Random.Range(1, int.MaxValue);
            var entity = _world.NewEntity();
            entity.Replace(new BoardComponent { State = SeededShuffle.Create(mode.Columns, mode.Rows, seed) });
            entity.Replace(new BoardHistoryComponent
            {
                Seed = seed,
                Moves = new List<Swap>(),
                StartTime = _time.RealtimeSinceStartup
            });
        }
    }
}
