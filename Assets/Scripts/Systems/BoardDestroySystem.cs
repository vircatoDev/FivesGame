using System;
using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.Systems
{
    class BoardDestroySystem : IEcsRunSystem
    {
        private EcsWorld _world;
        private readonly Transform _boardParent;

        private readonly EcsFilter<TileComponent> _tileFilter = null;
        private readonly EcsFilter<GameEndEvent> _gameEndEvent = null;

        public BoardDestroySystem(Transform boardParent)
        {
            _boardParent = boardParent;
        }

        public void Run()
        {
            if (_gameEndEvent.GetEntitiesCount() <= 0) return;

            DestroyBoardWithDelay(_gameEndEvent.Get1(0).Delay);
        }

        private async void DestroyBoardWithDelay(float delay)
        {
            await DestroyBoard(delay);
        }

        private async UniTask DestroyBoard(float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            DestroyTiles();
            DestroyGameObject();
        }

        private void DestroyGameObject()
        {
            Object.Destroy(_boardParent.GetChild(0).gameObject);
        }

        private void DestroyTiles()
        {
            foreach (var i in _tileFilter)
            {
                ref var tile = ref _tileFilter.Get1(i);

                Object.Destroy(tile.Rect.gameObject);

                _tileFilter.GetEntity(i).Destroy();
            }
        }
    }
}