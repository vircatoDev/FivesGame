using System.Collections.Generic;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    /// <summary>Draws the hint route when a hint is bought and hides it when the hint ends.</summary>
    public class BoardHintViewSystem : IEcsRunSystem
    {
        private readonly GameSession _session;
        private readonly EcsFilter<BoardComponent, BoardViewComponent> _boards;
        private readonly List<Vector2> _route = new List<Vector2>();
        private int _shownTile = -1;

        public void Run()
        {
            if (_boards.GetEntitiesCount() != 1)
            {
                _shownTile = -1;
                return;
            }

            var entity = _boards.GetEntity(0);
            var tile = entity.Has<BoardHintComponent>() ? entity.Get<BoardHintComponent>().TileId : -1;
            if (tile == _shownTile)
                return;

            _shownTile = tile;
            var view = _boards.Get2(0).View;
            if (tile < 0)
            {
                view.HideHint();
                return;
            }

            var mode = _session.SelectedGameMode;
            _route.Clear();
            foreach (var cell in BoardHint.PathOf(_boards.Get1(0).State, tile))
                _route.Add(mode.CellCenter(cell));
            view.ShowHint(_route);
        }
    }
}
