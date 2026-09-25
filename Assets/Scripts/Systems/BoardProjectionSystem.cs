using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Systems
{
    /// <summary>Projects the authoritative board into tile positions and animation requests.</summary>
    public class BoardProjectionSystem : IEcsRunSystem
    {
        private readonly EcsFilter<BoardComponent> _boards = null;
        private readonly EcsFilter<TileComponent> _tiles = null;
        private readonly EcsFilter<BoardInitializedEvent> _initialized = null;
        private readonly EcsFilter<BoardChangedEvent> _changes = null;

        public void Run()
        {
            if (_boards.GetEntitiesCount() != 1 || (_initialized.IsEmpty() && _changes.IsEmpty()))
                return;

            var board = _boards.Get1(0).State;
            // The first layout appears in place; later changes animate.
            var snap = !_initialized.IsEmpty();
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
