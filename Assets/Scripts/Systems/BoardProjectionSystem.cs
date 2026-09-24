using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Systems
{
    /// <summary>Projects the authoritative board into tile positions and animation requests.</summary>
    public class BoardProjectionSystem : IEcsRunSystem
    {
        private readonly GameSession _session;
        private readonly EcsFilter<BoardComponent> _boards;
        private readonly EcsFilter<TileComponent> _tiles;
        private readonly EcsFilter<BoardInitializedEvent> _initialized;
        private readonly EcsFilter<BoardChangedEvent> _changes;

        public void Run()
        {
            if (!_session.IsRunning || _boards.GetEntitiesCount() != 1)
                return;

            if (_initialized.GetEntitiesCount() == 0 && _changes.GetEntitiesCount() == 0)
                return;

            var board = _boards.Get1(0).State;
            // The first layout appears in place; later changes animate.
            var snap = _initialized.GetEntitiesCount() > 0;
            foreach (var i in _tiles)
            {
                ref var tile = ref _tiles.Get1(i);
                var cell = board.CellOf(tile.Id);
                if (snap || tile.Cell != cell)
                {
                    tile.Cell = cell;
                    _tiles.GetEntity(i).Replace(new MoveComponent
                    {
                        InstaMove = snap,
                        Duration = 0.35f,
                        TargetCell = cell
                    });
                }
            }
        }
    }
}
