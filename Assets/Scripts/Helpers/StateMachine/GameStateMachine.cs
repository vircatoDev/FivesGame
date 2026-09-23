using System;
using System.Collections.Generic;
using Scripts.Helpers.Factory;
using Scripts.Helpers.StateMachine.States;
using Scripts.Models;

namespace Scripts.Helpers.StateMachine
{
    /// <summary>Switches screens synchronously; UISystem applies the resulting screen events in the same frame.</summary>
    public class GameStateMachine
    {
        private readonly Dictionary<GameStateType, ScreenState> _states = new();
        private ScreenState _currentState;

        public GameStateMachine(GameStateFactory factory)
        {
            foreach (GameStateType stateType in Enum.GetValues(typeof(GameStateType)))
            {
                _states[stateType] = factory.Create(stateType);
            }
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
