using System.Collections.Generic;
using Fives.Configs;
using Fives.Models;
using Fives.UI;

namespace Fives.UI.Navigation
{
    /// <summary>
    /// Switches screens as the game state changes: GameStateSystem calls it in the frame the state changes, and the
    /// previous screen closes before the next one opens.
    /// </summary>
    public class GameStateMachine
    {
        private readonly Dictionary<GameStateType, ScreenState> _states = new Dictionary<GameStateType, ScreenState>();
        private ScreenState _currentState;

        public GameStateMachine(IReadOnlyDictionary<GameStateType, StateConfig> configs, IScreenNavigator navigator)
        {
            foreach (var config in configs.Values)
                _states[config.StateName] = new ScreenState(config, navigator);
        }

        public void ChangeState(GameStateType nextStateName)
        {
            var nextState = _states[nextStateName];
            _currentState?.Exit(nextState);
            nextState.Enter(_currentState);
            _currentState = nextState;
        }
    }
}
