using System.Collections.Generic;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    public class BoardSetupSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly GameSession _session;
        private readonly IFrameTime _time = null;
        private readonly EcsFilter<BoardComponent> _boards;
        private readonly EcsFilter<GameEndEvent> _ends;

        public void Run()
        {
            if (!_session.IsRunning || _ends.GetEntitiesCount() > 0 || _boards.GetEntitiesCount() > 0)
                return;

            var seed = Random.Range(1, int.MaxValue);
            var entity = _world.NewEntity();
            entity.Replace(new BoardComponent { State = SeededShuffle.Create(_session.SelectedGameMode.Columns, _session.SelectedGameMode.Rows, seed) });
            entity.Replace(new BoardHistoryComponent
            {
                Seed = seed,
                Moves = new List<Swap>(),
                Undone = new List<Swap>(),
                StartTime = _time.RealtimeSinceStartup
            });
        }
    }
}
