using DG.Tweening;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Systems
{
    public class BoardInitSystem : IEcsRunSystem
    {
        private readonly EcsFilter<BoardComponent>.Exclude<BoardViewComponent> _newBoards = null;

        private readonly EcsWorld _world;
        private readonly GlobalConfig _settings;
        private readonly GameSession _gameSession;
        private readonly Transform _boardParent;

        private GameObject _boardObject;

        public BoardInitSystem(Transform boardParent)
        {
            _boardParent = boardParent;
        }

        public void Run()
        {
            foreach (var i in _newBoards)
            {
                _newBoards.GetEntity(i).Get<BoardViewComponent>();
                CreateBoard();
                _world.Send<BoardInitializedEvent>();
            }
        }

        private void CreateBoard()
        {
            _boardObject = InstantiatePrefab(_settings.BoardPrefab, _boardParent);
            if (_boardObject == null) return;

            _boardObject.GetComponent<Image>().DOFade(1, 1).SetLink(_boardObject, LinkBehaviour.KillOnDestroy);

            var layout = _gameSession.SelectedGameMode;
            var source = _gameSession.SelectedPuzzle.Image;

            for (int i = 0; i < layout.BoardSize * layout.BoardSize; i++)
            {
                CreateTile(i, layout, source);
            }
        }

        private void CreateTile(int id, GameSettings layout, Sprite source)
        {
            var tileEntity = _world.NewEntity();
            ref var tileComponent = ref tileEntity.Get<TileComponent>();
            tileComponent.Id = id;
            tileComponent.Cell = id;

            var boardSize = layout.BoardSize;
            int row = id / boardSize;
            int column = id % boardSize;

            var tileObject = InstantiatePrefab(_settings.TilePrefab, _boardObject.transform.GetChild(0));
            if (tileObject == null) return;

            SetTileProperties(ref tileComponent, tileObject, layout.TileSize, layout.CellToAnchored(id));
            var sourceRect = source.rect;
            var width = sourceRect.width / boardSize;
            var height = sourceRect.height / boardSize;
            var region = new Rect(sourceRect.x + column * width,
                sourceRect.y + (boardSize - 1 - row) * height, width, height);
            var sprite = Sprite.Create(source.texture, region, new Vector2(0, 0), source.pixelsPerUnit);
            var tileUI = tileObject.GetComponent<TileUiProvider>();
            tileUI.Init(_world, id, sprite);
            tileUI.PlayTileShowAnimation();
        }

        private void SetTileProperties(ref TileComponent tileComponent, GameObject tileObject, float tileSize,
            Vector2 position)
        {
            var rectTransform = tileObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(tileSize, tileSize);
            rectTransform.anchoredPosition = position;
            tileComponent.Rect = rectTransform;
        }

        private GameObject InstantiatePrefab(string prefabPath, Transform parent)
        {
            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Prefab {prefabPath} missing");
                return null;
            }

            return UnityEngine.Object.Instantiate(prefab, parent);
        }
    }
}