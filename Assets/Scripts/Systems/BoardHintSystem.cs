using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using UnityEngine;

namespace Scripts.Systems
{
    /// <summary>
    /// Sells a hint for stars: the route home of one of the misplaced tiles closest to it, shown until the next move.
    /// One hint at a time; none while tiles are moving or after the win.
    /// </summary>
    public class BoardHintSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly GlobalConfig _config = null;
        private readonly EcsFilter<BoardComponent>.Exclude<BoardSolvedTag> _boards = null;
        private readonly EcsFilter<BoardControlEvent> _controls = null;
        private readonly EcsFilter<BoardChangedEvent> _changes = null;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves = null;
        private readonly EcsFilter<GameEndEvent> _ends = null;
        private readonly StarService _stars;

        public BoardHintSystem(StarService stars)
        {
            _stars = stars;
        }

        public void Run()
        {
            if (_boards.IsEmpty())
                return;

            var entity = _boards.GetEntity(0);
            if (_changes.GetEntitiesCount() > 0)
                entity.Del<BoardHintComponent>();

            foreach (var i in _controls)
            {
                if (_controls.Get1(i).Control == BoardControl.Hint)
                    TryBuy(entity, _boards.Get1(0).State);
            }
        }

        private void TryBuy(EcsEntity entity, BoardState board)
        {
            if (!_moves.IsEmpty() || !_ends.IsEmpty() || entity.Has<BoardHintComponent>())
                return;

            var candidates = BoardHint.ClosestMisplaced(board);
            if (candidates.Count == 0)
                return;

            if (!_stars.Spend(_config.HintPrice))
            {
                _world.Send(CurrencyChangedEvent.NotEnough(Currency.Stars));
                _world.PlaySound(AudioKeyCollection.WrongClick);
                return;
            }

            entity.Replace(new BoardHintComponent { TileId = candidates[Random.Range(0, candidates.Count)] });
            _world.Send(CurrencyChangedEvent.Changed(Currency.Stars, _stars.GetBalance(), -_config.HintPrice));
            _world.Send<SaveDataEvent>();
            _world.PlaySound(AudioKeyCollection.MenuClick);
        }
    }
}
