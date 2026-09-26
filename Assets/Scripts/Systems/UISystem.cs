using System.Collections.Generic;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Helpers.Factory;
using Scripts.Models;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.Systems
{
    /// <summary>Keeps one open screen per state. Closes are applied before opens, so a transition takes one frame.</summary>
    public class UISystem : IEcsRunSystem
    {
        private readonly EcsFilter<CloseAllScreensEvent> _closeAllFilter = null;
        private readonly EcsFilter<CloseScreenEvent> _closeFilter = null;
        private readonly EcsFilter<OpenScreenEvent> _openFilter = null;

        private readonly Dictionary<GameStateType, GameObject> _screens = new();
        private readonly Transform _uiRoot;
        private readonly Transform _popupLayer;
        private readonly ScreenCatalog _catalog;

        public UISystem(Transform uiRoot, Transform popupLayer, ScreenCatalog catalog)
        {
            _uiRoot = uiRoot;
            _popupLayer = popupLayer;
            _catalog = catalog;
        }

        public void Run()
        {
            if (_closeAllFilter.GetEntitiesCount() > 0)
            {
                foreach (var screen in _screens.Values)
                    Object.Destroy(screen);
                _screens.Clear();
            }

            foreach (var i in _closeFilter)
            {
                if (_screens.Remove(_closeFilter.Get1(i).State, out var screen))
                    Object.Destroy(screen);
            }

            foreach (var i in _openFilter)
                Open(_openFilter.Get1(i));
        }

        private void Open(in OpenScreenEvent evt)
        {
            var config = _catalog.Config(evt.State);
            if (_screens.ContainsKey(config.StateName))
                return;

            var screen = Object.Instantiate(config.ScreenPrefab, config.IsPopup ? _popupLayer : _uiRoot);
            _screens[config.StateName] = screen.gameObject;
            screen.Initialize(_catalog.Presenter(evt.State));
            screen.gameObject.SetActive(true);
        }
    }
}
