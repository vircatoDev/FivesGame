using System.Collections.Generic;
using Scripts.Configs;
using Scripts.Models;
using Scripts.UI.Presenters;
using VContainer;

namespace Scripts.Helpers.Factory
{
    /// <summary>
    /// Screen data per game state: its StateConfig and the one presenter for its screen. ECS events name a state;
    /// the presentation layer looks the screen up here, so events never carry UI objects.
    /// </summary>
    public class ScreenCatalog
    {
        private readonly Dictionary<GameStateType, StateConfig> _configs;
        private readonly Dictionary<GameStateType, BasePresenter> _presenters = new();
        private readonly IObjectResolver _container;

        public ScreenCatalog(Dictionary<GameStateType, StateConfig> configs, IObjectResolver container)
        {
            _configs = configs;
            _container = container;
        }

        public StateConfig Config(GameStateType state) => _configs[state];

        public BasePresenter Presenter(GameStateType state)
        {
            if (!_presenters.TryGetValue(state, out var presenter))
                _presenters[state] = presenter = Resolve(state);
            return presenter;
        }

        private BasePresenter Resolve(GameStateType state) => state switch
        {
            GameStateType.MainMenu => _container.Resolve<MainMenuPresenter>(),
            GameStateType.SelectMenu => _container.Resolve<SelectMenuPresenter>(),
            GameStateType.Playing => _container.Resolve<GamePlayPresenter>(),
            GameStateType.Finished => _container.Resolve<GameResultPresenter>(),
            GameStateType.Settings => _container.Resolve<SettingsPresenter>(),
            _ => throw new System.ArgumentOutOfRangeException(nameof(state), state, "No presenter for this state")
        };
    }
}
