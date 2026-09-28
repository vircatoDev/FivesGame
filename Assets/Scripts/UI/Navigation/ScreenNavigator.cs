using System.Collections.Generic;
using Fives.Configs;
using Fives.Models;
using Fives.UI.Presenters;
using Fives.UI.Views;
using UnityEngine;
using VContainer;

namespace Fives.UI.Navigation
{
    /// <summary>Where screens go: full screens under one layer, popups over them under another.</summary>
    public sealed class ScreenLayers
    {
        public readonly Transform Screens;
        public readonly Transform Popups;

        public ScreenLayers(Transform screens, Transform popups)
        {
            Screens = screens;
            Popups = popups;
        }
    }

    /// <summary>
    /// One open screen per state. A screen comes from its StateConfig prefab, and its presenter from the container by
    /// the type the view declares: menu presenters are created per opening, the gameplay presenter is the one singleton.
    /// </summary>
    public sealed class ScreenNavigator : IScreenNavigator
    {
        private readonly Dictionary<GameStateType, BaseView> _open = new Dictionary<GameStateType, BaseView>();
        private readonly IReadOnlyDictionary<GameStateType, StateConfig> _configs;
        private readonly ScreenLayers _layers;
        private readonly IObjectResolver _container;

        public ScreenNavigator(IReadOnlyDictionary<GameStateType, StateConfig> configs, ScreenLayers layers, IObjectResolver container)
        {
            _configs = configs;
            _layers = layers;
            _container = container;
        }

        public void Open(GameStateType state)
        {
            if (_open.ContainsKey(state))
                return;

            var config = _configs[state];
            var screen = Object.Instantiate(config.ScreenPrefab, config.IsPopup ? _layers.Popups : _layers.Screens);
            _open[state] = screen;
            screen.Initialize((BasePresenter)_container.Resolve(screen.PresenterType));
        }

        public void Close(GameStateType state)
        {
            if (_open.Remove(state, out var screen))
                Object.Destroy(screen.gameObject);
        }

        public void CloseAll()
        {
            foreach (var screen in _open.Values)
                Object.Destroy(screen.gameObject);
            _open.Clear();
        }
    }
}
