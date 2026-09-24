using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Systems
{
    /// <summary>
    /// Swaps neighboring tiles: tap one tile and then a neighbor, or swipe from a tile toward a neighbor.
    /// Also applies Undo and Redo. One input is accepted per frame, and none while tiles are moving.
    /// </summary>
    public class BoardInputSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly GameSession _session;
        private readonly EcsFilter<BoardComponent, BoardHistoryComponent> _boards;
        private readonly EcsFilter<TileClickEvent> _clicks;
        private readonly EcsFilter<TileSwipeEvent> _swipes;
        private readonly EcsFilter<BoardControlEvent> _controls;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves;
        private readonly EcsFilter<GameEndEvent> _ends;
        private readonly EcsFilter<BoardInitializedEvent> _initialized;

        public void Run()
        {
            if (!_session.IsRunning || _session.IsCompleted || _boards.GetEntitiesCount() != 1
                || _ends.GetEntitiesCount() > 0 || _initialized.GetEntitiesCount() > 0 || _moves.GetEntitiesCount() > 0)
                return;

            foreach (var i in _controls)
            {
                ApplyControl(_controls.Get1(i).Control);
                return;
            }

            foreach (var i in _swipes)
            {
                ref var swipe = ref _swipes.Get1(i);
                OnSwipe(swipe.Id, swipe.Dx, swipe.Dy);
                return;
            }

            foreach (var i in _clicks)
            {
                OnTap(_clicks.Get1(i).Id);
                return;
            }
        }

        private void OnTap(int tileId)
        {
            var entity = _boards.GetEntity(0);
            var board = _boards.Get1(0).State;
            var cell = board.CellOf(tileId);
            if (cell < 0)
                return;

            if (entity.Has<TileSelectionComponent>())
            {
                var selectedCell = board.CellOf(entity.Get<TileSelectionComponent>().TileId);
                if (board.AreNeighbors(selectedCell, cell))
                {
                    TrySwap(new Swap(selectedCell, cell));
                    return;
                }

                if (selectedCell == cell)
                {
                    entity.Del<TileSelectionComponent>();
                    _world.PlaySound(AudioKeyCollection.MenuClick);
                    return;
                }
            }

            entity.Replace(new TileSelectionComponent { TileId = tileId });
            _world.PlaySound(AudioKeyCollection.MenuClick);
        }

        private void OnSwipe(int tileId, int dx, int dy)
        {
            var board = _boards.Get1(0).State;
            var cell = board.CellOf(tileId);
            var column = cell % board.Size + dx;
            var row = cell / board.Size + dy;
            var inside = cell >= 0 && column >= 0 && column < board.Size && row >= 0 && row < board.Size;

            if (!inside || !TrySwap(new Swap(cell, row * board.Size + column)))
                _world.PlaySound(AudioKeyCollection.WrongClick);
        }

        private bool TrySwap(Swap swap)
        {
            var board = _boards.Get1(0).State;
            if (board.IsSolved || !board.TrySwap(swap))
                return false;

            ref var history = ref _boards.Get2(0);
            history.Moves.Add(swap);
            history.Undone.Clear();
            _boards.GetEntity(0).Del<TileSelectionComponent>();
            _world.Send<BoardChangedEvent>();
            _world.PlaySound(AudioKeyCollection.RightTap);
            return true;
        }

        // A swap is its own inverse: Undo re-applies the last move, Redo re-applies the last undone one.
        private void ApplyControl(BoardControl control)
        {
            ref var history = ref _boards.Get2(0);
            var (from, to) = control == BoardControl.Undo ? (history.Moves, history.Undone) : (history.Undone, history.Moves);
            if (from.Count == 0)
                return;

            var swap = from[from.Count - 1];
            _boards.Get1(0).State.TrySwap(swap);
            from.RemoveAt(from.Count - 1);
            to.Add(swap);
            _boards.GetEntity(0).Del<TileSelectionComponent>();
            _world.Send<BoardChangedEvent>();
        }
    }
}
