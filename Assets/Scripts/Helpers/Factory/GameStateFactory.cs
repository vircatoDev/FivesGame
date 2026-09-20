using System;
using System.Collections.Generic;
using Scripts.Configs;
using Scripts.Helpers.StateMachine.States;
using Scripts.Models;
using Scripts.Services;

namespace Scripts.Helpers.Factory
{
    public class GameStateFactory
    {
        private readonly Dictionary<GameStateType, StateConfig> _statesConfigCollection;
        private readonly ECSCommandService _ecsCommandService;
        private readonly PresenterFactory _presenterFactory;

        private readonly Dictionary<GameStateType, IGameState> _states = new();

        public GameStateFactory(Dictionary<GameStateType, StateConfig> statesConfigCollection,
            ECSCommandService ecsCommandService, PresenterFactory presenterFactory)
        {
            _statesConfigCollection = statesConfigCollection;
            _ecsCommandService = ecsCommandService;
            _presenterFactory = presenterFactory;
        }

        public IGameState GetState(GameStateType stateType)
        {
            if (!_states.ContainsKey(stateType))
            {
                _states[stateType] = CreateState(stateType);
            }

            return _states[stateType];
        }

        private IGameState CreateState(GameStateType stateType)
        {
            var stateConfig = _statesConfigCollection[stateType];
            var presenter = _presenterFactory.CreatePresenter(stateType);

            return stateType switch
            {
                GameStateType.MainMenu => new MainMenuState(_ecsCommandService, stateConfig, presenter),
                GameStateType.SelectMenu => new SelectMenuState(_ecsCommandService, stateConfig, presenter),
                GameStateType.Playing => new PlayingState(_ecsCommandService, stateConfig, presenter),
                GameStateType.Finished => new GameResultState(_ecsCommandService, stateConfig, presenter),
                GameStateType.Settings => new SettingsMenuState(_ecsCommandService, stateConfig, presenter),
                _ => throw new Exception($"State for {stateType} not found.")
            };
        }
    }
}