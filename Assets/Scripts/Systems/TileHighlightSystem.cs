using Leopotam.Ecs;
using Scripts.Components;
using Scripts.UI;

namespace Scripts.Systems
{
    /// <summary>Raises the selected tile and lowers it again when the selection changes.</summary>
    public class TileHighlightSystem : IEcsRunSystem
    {
        private readonly EcsFilter<BoardComponent, TileSelectionComponent> _selections = null;
        private readonly EcsFilter<TileComponent> _tiles = null;
        private int _highlighted = -1;

        public void Run()
        {
            var selected = _selections.GetEntitiesCount() > 0 ? _selections.Get2(0).TileId : -1;
            if (selected == _highlighted)
                return;

            foreach (var i in _tiles)
            {
                ref var tile = ref _tiles.Get1(i);
                if (tile.Id == _highlighted || tile.Id == selected)
                    tile.Rect.GetComponent<TileUiProvider>().SetSelected(tile.Id == selected);
            }

            _highlighted = selected;
        }
    }
}
