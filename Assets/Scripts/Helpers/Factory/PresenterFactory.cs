using Scripts.Models;
using Scripts.UI.Presenters;
using VContainer;

namespace Scripts.Helpers.Factory
{
    public class PresenterFactory
    {
        private readonly IObjectResolver _container;

        public PresenterFactory(IObjectResolver container)
        {
            _container = container;
        }

        public BasePresenter CreatePresenter(GameStateType stateType)
        {
            return stateType switch
            {
                GameStateType.MainMenu => _container.Resolve<MainMenuPresenter>(),
                GameStateType.SelectMenu => _container.Resolve<SelectMenuPresenter>(),
                GameStateType.Playing => _container.Resolve<GamePlayPresenter>(),
                GameStateType.Finished => _container.Resolve<GameResultPresenter>(),
                GameStateType.Settings => _container.Resolve<SettingsPresenter>(),
                _ => throw new System.Exception($"No presenter found for {stateType}")
            };
        }
    }
}