using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Systems
{
    public class BoardInputSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly GameSession _session;
        private readonly EcsFilter<BoardComponent, BoardHistoryComponent> _boards;
        private readonly EcsFilter<TileClickEvent> _clicks;
        private readonly EcsFilter<BoardControlEvent> _controls;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves;
        private readonly EcsFilter<GameEndEvent> _ends;
        private readonly EcsFilter<BoardInitializedEvent> _initialized;
        private readonly EcsFilter<GameStateComponent> _states;

        public void Run()
        {
            if (!_session.IsRunning || _session.IsCompleted || _boards.GetEntitiesCount() != 1
                || _states.Get1(0).CurrentState != GameStateType.Playing
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
                    _world.NewEntity().Get<BoardRefreshEvent>();
                }
                else if (_moves.GetEntitiesCount() == 0 && !replaying)
                {
                    ApplyControl(control);
                }
                return;
            }

            if (replaying || _moves.GetEntitiesCount() > 0)
                return;

            foreach (var i in _clicks)
            {
                var accepted = TryMoveTile(_clicks.Get1(i).Id);

                _world.NewEntity().Replace(new PlaySoundEffectEvent
                {
                    Key = accepted ? AudioKeyCollection.RightTap : AudioKeyCollection.WrongClick,
                    Volume = 1f
                });
                return;
            }
        }

        private bool TryMoveTile(int tileId)
        {
            var board = _boards.Get1(0).State;
            if (board.IsSolved)
                return false;

            for (var cell = 0; cell < board.CellCount; cell++)
            {
                if (board[cell] != tileId)
                    continue;
                if (!board.TryMove(cell))
                    return false;
                _boards.Get2(0).Moves.Add(cell);
                return true;
            }
            return false;
        }

        private void ApplyControl(BoardControl control)
        {
            var board = _boards.Get1(0).State;
            ref var history = ref _boards.Get2(0);
            var count = history.Moves.Count;
            if (count == 0)
                return;

            switch (control)
            {
                case BoardControl.Undo:
                    // An accepted move leaves the empty cell at its recorded source cell.
                    var previousEmptyCell = count == 1 ? history.InitialEmptyCell : history.Moves[count - 2];
                    board.TryMove(previousEmptyCell);
                    history.Moves.RemoveAt(count - 1);
                    break;
                case BoardControl.Replay:
                    var data = new ReplayData(ReplayData.CurrentVersion, board.Size, board.EmptyTileId,
                        history.Seed, history.ShuffleSteps, history.Moves);
                    _boards.GetEntity(0).Replace(new BoardReplayComponent
                    {
                        Data = data,
                        State = SeededShuffle.Create(data.Size, data.EmptyTileId, data.Seed, data.ShuffleSteps)
                    });
                    _world.NewEntity().Get<BoardRefreshEvent>();
                    break;
            }
        }
    }
}
