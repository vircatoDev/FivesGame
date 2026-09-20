using System.Collections.Generic;
using System.Linq;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Helpers;
using Scripts.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Systems
{
    class ShuffleSystem : IEcsRunSystem
    {
        private readonly EcsFilter<BoardInitializedEvent> _boardInitFilter = null;
        private readonly EcsFilter<TileComponent> _tileFilter = null;
        private GameSession _gameSession;

        public void Run()
        {
            if (_boardInitFilter.GetEntitiesCount() == 0) return;

            var positions = new List<Vector2>();
            for (int row = 0; row < _gameSession.SelectedGameMode.BoardSize; row++)
            {
                for (int col = 0; col < _gameSession.SelectedGameMode.BoardSize; col++)
                {
                    positions.Add(new Vector2Int(col, row));
                }
            }

            List<Vector2> startField = PuzzleGenerator.GenerateStartField(out var emptyIndexResult);
            positions = startField;

            var emptyTile = _tileFilter.GetEntity(emptyIndexResult);
            emptyTile.Get<EmptyTileComponent>();

            var index = 0;

            foreach (var i in _tileFilter)
            {
                ref var tile = ref _tileFilter.Get1(i);

                var nextPosition = positions[index++];
                tile.Position = new Vector3(nextPosition.x, nextPosition.y, 0f);

                var moveEntity = _tileFilter.GetEntity(i);

                if (i == emptyIndexResult)
                {
                    tile.isEmpty = true;
                    tile.Rect.GetComponentsInChildren<Image>().ToList().ForEach(x =>
                    {
                        x.raycastTarget = false;
                        x.color = Color.clear;
                    });
                }

                moveEntity.Replace(new MoveComponent
                {
                    InstaMove = true,
                    Direction = Vector3.zero,
                    Speed = 0f,
                    TargetPosition = tile.Position
                });
            }
        
            foreach (var i in _boardInitFilter)
            {
                _boardInitFilter.GetEntity(i).Destroy();
            }
        }
    }
}