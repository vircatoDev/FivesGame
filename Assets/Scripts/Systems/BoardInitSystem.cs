using DG.Tweening;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
            var view = Object.Instantiate(_settings.BoardPrefab, _boardParent);
            view.GetComponent<Image>().DOFade(1, 1).SetLink(view.gameObject, LinkBehaviour.KillOnDestroy);

            var layout = _gameSession.SelectedGameMode;
            var source = _gameSession.PuzzleImage;
            var picture = PictureUv(source, (float)layout.Columns / layout.Rows);

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

            var tile = Object.Instantiate(_settings.TilePrefab, parent);
            var rect = (RectTransform)tile.transform;
            rect.sizeDelta = new Vector2(layout.TileSize, layout.TileSize);
            rect.anchoredPosition = layout.CellToAnchored(id);
            tileComponent.Rect = rect;

            // The picture cut into cells; rows count from the top, UVs from the bottom.
            var width = picture.width / layout.Columns;
            var height = picture.height / layout.Rows;
            tile.Init(_world, id, texture, new Rect(picture.x + column * width, picture.y + (layout.Rows - 1 - row) * height, width, height));
            tile.PlayTileShowAnimation();
        }
    }
}