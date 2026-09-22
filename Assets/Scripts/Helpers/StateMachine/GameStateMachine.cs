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
        private bool _isTransitioning;
        private GameStateType? _pendingState;

        public GameStateMachine(GameStateFactory factory)
        {
            foreach (GameStateType stateType in Enum.GetValues(typeof(GameStateType)))
            {
                _states[stateType] = factory.GetState(stateType);
            }
        }

        public void ChangeState(GameStateType nextStateName)
        {
            _pendingState = nextStateName;

            if (!_isTransitioning)
            {
                ProcessTransitions().Forget();
            }
        }

        private async UniTask ProcessTransitions()
        {
            _isTransitioning = true;

            try
            {
                while (_pendingState.HasValue)
                {
                    var nextState = _pendingState.Value;
                    _pendingState = null;
                    await TransitionTo(nextState);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private async UniTask TransitionTo(GameStateType nextStateName)
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
