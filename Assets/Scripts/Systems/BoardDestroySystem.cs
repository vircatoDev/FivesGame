using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.Systems
{
    class BoardDestroySystem : IEcsRunSystem
    {
        private readonly Transform _boardParent;
        private readonly GameSession _session;

        private readonly EcsFilter<BoardComponent> _boards;
        private readonly EcsFilter<TileComponent> _tileFilter = null;
        private readonly EcsFilter<GameEndEvent> _gameEndEvent = null;

        public BoardDestroySystem(Transform boardParent)
        {
            _boardParent = boardParent;
        }

        public void Run()
        {
            if (_gameEndEvent.GetEntitiesCount() > 0)
            {
                foreach (var i in _boards)
                    _boards.GetEntity(i).Destroy();
                DestroyCurrentBoard();
                _session.EndRun();
            }
        }

        private void DestroyCurrentBoard()
        {
            if (_boardParent.childCount == 0)
            {
                return;
            }

            var board = _boardParent.GetChild(_boardParent.childCount - 1);

            foreach (var i in _tileFilter)
            {
                var tile = _tileFilter.Get1(i);
                if (tile.Rect != null && tile.Rect.IsChildOf(board))
                {
                    _tileFilter.GetEntity(i).Destroy();
                }
            }

            Object.Destroy(board.gameObject);
        }
    }
}
