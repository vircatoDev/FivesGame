using System.Collections.Generic;
using Scripts.Configs;
using Scripts.Models;
using Scripts.UI.Presenters;
using VContainer;

namespace Scripts.Helpers.Factory
{
    /// <summary>
    /// Screen data per game state: its StateConfig and a presenter for each opening of its screen. ECS events name a
    /// state; the presentation layer looks the screen up here, so events never carry UI objects.
    /// </summary>
    public class ScreenCatalog
    {
        private readonly Dictionary<GameStateType, StateConfig> _configs;
        private readonly IObjectResolver _container;

        public ScreenCatalog(Dictionary<GameStateType, StateConfig> configs, IObjectResolver container)
        {
            _configs = configs;
            _container = container;
        }

        public StateConfig Config(GameStateType state) => _configs[state];

        /// <summary>
        /// A presenter for a screen being opened. Menu presenters are transient and live as long as their screen,
        /// so nothing async outlives it; the gameplay presenter is the one singleton, as the HUD system holds it.
        /// </summary>
        public BasePresenter Presenter(GameStateType state) => state switch
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
