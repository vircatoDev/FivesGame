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
            var config = _configs[stateType];
            var presenter = _presenterFactory.CreatePresenter(stateType);

            return stateType == GameStateType.Playing
                ? new PlayingState(_world, config, presenter)
                : new ScreenState(_world, config, presenter);
        }
    }
}
