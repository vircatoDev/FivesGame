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

        public BoardInitSystem(Transform boardParent)
        {
            _boardParent = boardParent;
        }

        public void Run()
        {
            foreach (var i in _newBoards)
            {
                _newBoards.GetEntity(i).Get<BoardViewComponent>().View = CreateBoard();
                _world.Send<BoardInitializedEvent>();
            }
        }

        private BoardView CreateBoard()
        {
            var boardObject = InstantiatePrefab(_settings.BoardPrefab, _boardParent);
            if (boardObject == null) return null;

            boardObject.GetComponent<Image>().DOFade(1, 1).SetLink(boardObject, LinkBehaviour.KillOnDestroy);

            var layout = _gameSession.SelectedGameMode;
            var source = _gameSession.SelectedPuzzle.Image;
            var picture = PictureUv(source, (float)layout.Columns / layout.Rows);

            var view = boardObject.GetComponent<BoardView>();
            view.SetPicture(source.texture, picture, layout.BoardArea);
            for (int i = 0; i < layout.Columns * layout.Rows; i++)
            {
                CreateTile(i, layout, view.Tiles, source.texture, picture);
            }

            return view;
        }

        /// <summary>UV rect of the largest centered part of the sprite with the board's aspect ratio.</summary>
        private static Rect PictureUv(Sprite sprite, float aspect)
        {
            var rect = sprite.textureRect;
            var texture = sprite.texture;
            var width = Mathf.Min(rect.width, rect.height * aspect);
            var height = width / aspect;
            return new Rect((rect.x + (rect.width - width) / 2) / texture.width,
                (rect.y + (rect.height - height) / 2) / texture.height, width / texture.width, height / texture.height);
        }

        private void CreateTile(int id, GameSettings layout, Transform parent, Texture texture, Rect picture)
        {
            var tileEntity = _world.NewEntity();
            ref var tileComponent = ref tileEntity.Get<TileComponent>();
            tileComponent.Id = id;
            tileComponent.Cell = id;

            int row = id / layout.Columns;
            int column = id % layout.Columns;

            var tileObject = InstantiatePrefab(_settings.TilePrefab, parent);
            if (tileObject == null) return;

            SetTileProperties(ref tileComponent, tileObject, layout.TileSize, layout.CellToAnchored(id));
            // The picture cut into cells; rows count from the top, UVs from the bottom.
            var width = picture.width / layout.Columns;
            var height = picture.height / layout.Rows;
            var uvRect = new Rect(picture.x + column * width, picture.y + (layout.Rows - 1 - row) * height, width, height);
            var tileUI = tileObject.GetComponent<TileUiProvider>();
            tileUI.Init(_world, id, texture, uvRect);
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