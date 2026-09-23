using System.Collections.Generic;
using Leopotam.Ecs;
using Scripts.Configs;
using Scripts.Helpers.StateMachine.States;
using Scripts.Models;

namespace Scripts.Helpers.Factory
{
    public class GameStateFactory
    {
        private readonly Dictionary<GameStateType, StateConfig> _configs;
        private readonly EcsWorld _world;
        private readonly PresenterFactory _presenterFactory;

        public GameStateFactory(Dictionary<GameStateType, StateConfig> configs, EcsWorld world,
            PresenterFactory presenterFactory)
        {
            _configs = configs;
            _world = world;
            _presenterFactory = presenterFactory;
        }

        public ScreenState Create(GameStateType stateType)
        {
            return new ScreenState(_world, _configs[stateType], _presenterFactory.CreatePresenter(stateType));
        }
    }
}
