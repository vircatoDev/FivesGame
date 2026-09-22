using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Systems
{
    public class BoardReplaySystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter<BoardReplayComponent> _replays;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves;
        private readonly EcsFilter<BoardRefreshEvent> _refresh;
        private readonly EcsFilter<GameEndEvent> _ends;
        private readonly EcsFilter<GameStateComponent> _states;

        public void Run()
        {
            if (_states.Get1(0).CurrentState != GameStateType.Playing || _ends.GetEntitiesCount() > 0
                || _moves.GetEntitiesCount() > 0 || _refresh.GetEntitiesCount() > 0)
                return;

            foreach (var i in _replays)
            {
                ref var replay = ref _replays.Get1(i);
                if (replay.Position == replay.Data.Moves.Count)
                {
                    _replays.GetEntity(i).Del<BoardReplayComponent>();
                    _world.NewEntity().Get<BoardRefreshEvent>();
                    continue;
                }

                replay.State.TryMove(replay.Data.Moves[replay.Position]);
                replay.Position++;
            }
        }
    }
}
