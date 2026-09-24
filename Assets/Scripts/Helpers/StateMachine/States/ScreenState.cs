using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Helpers.StateMachine.States
{
    /// <summary>A game state that shows its screen on enter and closes it on exit, as described by its StateConfig.</summary>
    public class ScreenState
    {
        private readonly EcsWorld _world;
        private readonly StateConfig _config;
        private GameStateType _prevStateName = GameStateType.MainMenu;

        public ScreenState(EcsWorld world, StateConfig config)
        {
            _world = world;
            _config = config;
        }

        public bool IsPopup => _config.IsPopup;
        public GameStateType StateName => _config.StateName;

        public void Enter(ScreenState prevState)
        {
            if (prevState != null)
                _prevStateName = prevState.StateName;

            _world.Send(new OpenScreenEvent { State = StateName });
        }

        /// <summary>A popup opens over this screen; leaving a popup for the screen below closes only the popup.</summary>
        public void Exit(ScreenState nextState)
        {
            if (nextState.IsPopup)
                return;

            if (_prevStateName == nextState.StateName)
                _world.Send(new CloseScreenEvent { State = StateName });
            else
                _world.Send<CloseAllScreensEvent>();
        }
    }
}
