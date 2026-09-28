using Fives.Configs;
using Fives.Models;
using Fives.UI;

namespace Fives.UI.Navigation
{
    /// <summary>A game state that shows its screen on enter and closes it on exit, as described by its StateConfig.</summary>
    public class ScreenState
    {
        private readonly StateConfig _config;
        private readonly IScreenNavigator _navigator;
        private GameStateType _prevStateName = GameStateType.MainMenu;

        public ScreenState(StateConfig config, IScreenNavigator navigator)
        {
            _config = config;
            _navigator = navigator;
        }

        public bool IsPopup => _config.IsPopup;
        public GameStateType StateName => _config.StateName;

        public void Enter(ScreenState prevState)
        {
            if (prevState != null)
                _prevStateName = prevState.StateName;

            _navigator.Open(StateName);
        }

        /// <summary>A popup opens over this screen; leaving a popup for the screen below closes only the popup.</summary>
        public void Exit(ScreenState nextState)
        {
            if (nextState.IsPopup)
                return;

            if (_prevStateName == nextState.StateName)
                _navigator.Close(StateName);
            else
                _navigator.CloseAll();
        }
    }
}
