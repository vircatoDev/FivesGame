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
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;

        private readonly EcsWorld _world;
        private readonly GlobalConfig _settings;
        private readonly GameSession _gameSession;
        private readonly Transform _boardParent;

        private bool _boardWasCreated;
        private GameSettings _roundSetting;
        private GameObject _boardObject;

        public BoardInitSystem(Transform boardParent)
        {
            _boardParent = boardParent;
        }

        public void Run()
        {
            if (_stateFilter.Get1(0).CurrentState != GameStateType.Playing)
            {
                _boardWasCreated = false;
                return;
            }

            if (!_boardWasCreated)
            {
                _boardWasCreated = true;
                CreateBoard();
                _world.NewEntity().Get<BoardInitializedEvent>();
            }
        }

        private void CreateBoard()
        {
            _roundSetting = _gameSession.SelectedGameMode;
            _boardObject = InstantiatePrefab(_settings.BoardPrefab, _boardParent);
            if (_boardObject == null) return;

            _boardObject.GetComponent<Image>().DOFade(1, 1).SetLink(_boardObject, LinkBehaviour.KillOnDestroy);

            int boardSize = _roundSetting.BoardSize;
            float tileSize = _roundSetting.TileSize;
            float spacing = _roundSetting.TileSpacing;
            Vector2 startPosition = GetTopLeftCorner(_boardParent);

            var source = _gameSession.SelectedPuzzle.Image;

            for (int i = 0; i < boardSize * boardSize; i++)
            {
                CreateTile(i, boardSize, tileSize, spacing, startPosition, source);
            }
        }

        private void CreateTile(int id, int boardSize, float tileSize, float spacing, Vector2 startPosition,
            Sprite source)
        {
            var tileEntity = _world.NewEntity();
            ref var tileComponent = ref tileEntity.Get<TileComponent>();
            tileComponent.Id = id;

            int row = id / boardSize;
            int column = id % boardSize;

            Vector2 position = startPosition + new Vector2(
                column * (tileSize + spacing),
                -row * (tileSize + spacing)
            );

            var tileObject = InstantiatePrefab(_settings.TilePrefab, _boardObject.transform.GetChild(0));
            if (tileObject == null) return;

            SetTileProperties(ref tileComponent, tileObject, tileSize, position);
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

        private Vector2 GetTopLeftCorner(Transform board)
        {
            var rectTransform = board.GetComponent<RectTransform>();

            var pivotOffset = new Vector2(
                rectTransform.rect.width * rectTransform.pivot.x,
                rectTransform.rect.height * rectTransform.pivot.y
            );

            var worldPosition = rectTransform.position;
            var localPosition = (Vector2)worldPosition - pivotOffset;

            return new Vector2(
                localPosition.x + rectTransform.rect.width * (1 - rectTransform.pivot.x),
                localPosition.y - rectTransform.rect.height * rectTransform.pivot.y
            );
        }
    }
}