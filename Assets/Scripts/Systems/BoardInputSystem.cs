using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Systems
{
    /// <summary>
    /// Swaps neighboring tiles: tap one tile and then a neighbor, or swipe from a tile toward a neighbor.
    /// Also applies Undo (hints belong to BoardHintSystem). One input is accepted per frame, and none while tiles are moving.
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
            var column = cell % board.Columns + dx;
            var row = cell / board.Columns + dy;
            var inside = cell >= 0 && column >= 0 && column < board.Columns && row >= 0 && row < board.Rows;

            if (!inside || !TrySwap(new Swap(cell, row * board.Columns + column)))
                _world.PlaySound(AudioKeyCollection.WrongClick);
        }

        private bool TrySwap(Swap swap)
        {
            var board = _boards.Get1(0).State;
            if (board.IsSolved || !board.TrySwap(swap))
                return false;

            ref var history = ref _boards.Get2(0);
            history.Moves.Add(swap);
            _boards.GetEntity(0).Del<TileSelectionComponent>();
            _world.Send<BoardChangedEvent>();
            _world.PlaySound(AudioKeyCollection.RightTap);
            return true;
        }

        // A swap is its own inverse: Undo re-applies the last move and forgets it.
        private void ApplyControl(BoardControl control)
        {
            var moves = _boards.Get2(0).Moves;
            if (control != BoardControl.Undo || moves.Count == 0)
                return;

            _boards.Get1(0).State.TrySwap(moves[moves.Count - 1]);
            moves.RemoveAt(moves.Count - 1);
            _boards.GetEntity(0).Del<TileSelectionComponent>();
            _world.Send<BoardChangedEvent>();
        }
    }
}
