using System.Collections.Generic;
using Leopotam.Ecs;
using Scripts.Components;
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

        public UISystem(Transform uiRoot, Transform popupLayer)
        {
            _uiRoot = uiRoot;
            _popupLayer = popupLayer;
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
            var config = evt.Config;
            if (_screens.ContainsKey(config.StateName))
                return;

            var prefab = Resources.Load<GameObject>(config.ScreenPrefab);
            if (prefab == null)
            {
                Debug.LogError($"Screen prefab {config.ScreenPrefab} not found!");
                return;
            }

            var screen = Object.Instantiate(prefab, config.IsPopup ? _popupLayer : _uiRoot);
            _screens[config.StateName] = screen;
            screen.GetComponent<BaseView>().Initialize(evt.Presenter);
            screen.SetActive(true);
        }
    }
}
