using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Systems
{
    /// <summary>
    /// Swaps neighboring tiles: tap one tile and then a neighbor, or swipe from a tile toward a neighbor.
    /// Also applies Undo and Replay. One input is accepted per frame, and none while tiles are moving.
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
                || _ends.GetEntitiesCount() > 0 || _initialized.GetEntitiesCount() > 0)
                return;

            var entity = _boards.GetEntity(0);
            var replaying = entity.Has<BoardReplayComponent>();
            // Stopping a replay can interrupt an animation: projection will snap to the live board.
            foreach (var i in _controls)
            {
                var control = _controls.Get1(i).Control;
                if (control == BoardControl.StopReplay && replaying)
                {
                    entity.Del<BoardReplayComponent>();
                    _world.Send(new BoardChangedEvent { Snap = true });
                }
                else if (_moves.GetEntitiesCount() == 0 && !replaying)
                {
                    ApplyControl(control);
                }
                return;
            }

            if (replaying || _moves.GetEntitiesCount() > 0)
                return;

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

            _boards.Get2(0).Moves.Add(swap);
            _boards.GetEntity(0).Del<TileSelectionComponent>();
            _world.Send<BoardChangedEvent>();
            _world.PlaySound(AudioKeyCollection.RightTap);
            return true;
        }

        private void ApplyControl(BoardControl control)
        {
            var entity = _boards.GetEntity(0);
            var board = _boards.Get1(0).State;
            ref var history = ref _boards.Get2(0);
            var count = history.Moves.Count;
            if (count == 0)
                return;

            entity.Del<TileSelectionComponent>();
            switch (control)
            {
                case BoardControl.Undo:
                    // A swap is its own inverse.
                    board.TrySwap(history.Moves[count - 1]);
                    history.Moves.RemoveAt(count - 1);
                    _world.Send<BoardChangedEvent>();
                    break;
                case BoardControl.Replay:
                    var data = new ReplayData(ReplayData.CurrentVersion, board.Size, history.Seed, history.Moves);
                    entity.Replace(new BoardReplayComponent
                    {
                        Data = data,
                        State = data.CreatePlaybackBoard()
                    });
                    _world.Send(new BoardChangedEvent { Snap = true });
                    break;
            }
        }
    }
}
