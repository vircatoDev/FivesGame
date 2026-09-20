using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scripts.Helpers.Factory;
using Scripts.Helpers.StateMachine.States;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Helpers.StateMachine
{
    public class GameStateMachine
    {
        private IGameState _currentState;
        private readonly Dictionary<GameStateType, IGameState> _states = new();

        public GameStateMachine(GameStateFactory factory)
        {
            foreach (GameStateType stateType in Enum.GetValues(typeof(GameStateType)))
            {
                _states[stateType] = factory.GetState(stateType);
            }
        }

        public async UniTask ChangeState(GameStateType nextStateName)
        {
            if (!_states.TryGetValue(nextStateName, out var nextState))
            {
                Debug.LogError($"State {nextStateName} not found");
                return;
            }

            if (_currentState != null)
            {
                await _currentState.Exit(nextState);
            }
        
            await nextState.Enter(_currentState);
        
            _currentState = nextState;

        }
    }
}