using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    /// <summary>Projects the authoritative board into tile positions and animation requests.</summary>
    public class BoardProjectionSystem : IEcsRunSystem
    {
        private readonly GameSession _session;
        private readonly EcsFilter<BoardComponent> _boards;
        private readonly EcsFilter<BoardReplayComponent> _replays;
        private readonly EcsFilter<TileComponent> _tiles;
        private readonly EcsFilter<BoardInitializedEvent> _initialized;
        private readonly EcsFilter<BoardChangedEvent> _changes;

        public void Run()
        {
            if (!_session.IsRunning || _boards.GetEntitiesCount() != 1)
                return;

            if (_initialized.GetEntitiesCount() == 0 && _changes.GetEntitiesCount() == 0)
                return;

            var board = _replays.GetEntitiesCount() > 0 ? _replays.Get1(0).State : _boards.Get1(0).State;
            var initial = _initialized.GetEntitiesCount() > 0;
            var snap = initial;
            foreach (var i in _changes)
                snap |= _changes.Get1(i).Snap;
            foreach (var i in _tiles)
            {
                ref var tile = ref _tiles.Get1(i);
                var empty = tile.Id == board.EmptyTileId;
                if (initial && empty)
                {
                    foreach (var graphic in tile.Rect.GetComponentsInChildren<UnityEngine.UI.Graphic>())
                    {
                        graphic.raycastTarget = false;
                        graphic.color = Color.clear;
                    }
                }

                var cell = board.CellOf(tile.Id);
                if (snap || tile.Cell != cell)
                {
                    tile.Cell = cell;
                    _tiles.GetEntity(i).Replace(new MoveComponent
                    {
                        InstaMove = snap || empty,
                        Duration = 0.35f,
                        TargetCell = cell
                    });
                }
            }
        }
    }
}
