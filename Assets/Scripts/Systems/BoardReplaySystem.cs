using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Systems
{
    public class BoardReplaySystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter<BoardReplayComponent> _replays;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves;
        private readonly EcsFilter<BoardChangedEvent> _changes;
        private readonly EcsFilter<GameEndEvent> _ends;

        public void Run()
        {
            if (_ends.GetEntitiesCount() > 0 || _moves.GetEntitiesCount() > 0 || _changes.GetEntitiesCount() > 0)
                return;

            foreach (var i in _replays)
            {
                ref var replay = ref _replays.Get1(i);
                if (replay.Position == replay.Data.Moves.Count)
                {
                    _replays.GetEntity(i).Del<BoardReplayComponent>();
                    _world.Send(new BoardChangedEvent { Snap = true });
                    continue;
                }

                replay.State.TrySwap(replay.Data.Moves[replay.Position]);
                replay.Position++;
                _world.Send<BoardChangedEvent>();
            }
        }
    }
}
