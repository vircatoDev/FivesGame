using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Helpers.StateMachine;
using Scripts.Models;

namespace Scripts.Systems
{
    /// <summary>Applies the latest state request of the frame to both GameStateComponent and the screen state machine.</summary>
    public class GameStateSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsFilter<ChangeStateEvent> _stateChangeFilter = null;
        private readonly EcsFilter<GameStateComponent> _gameStateFilter = null;
        private readonly GameStateMachine _stateMachine;

        public GameStateSystem(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Init()
        {
            _stateMachine.ChangeState(GameStateType.MainMenu);
        }

        public void Run()
        {
            var count = _stateChangeFilter.GetEntitiesCount();
            if (count == 0)
                return;

            var nextState = _stateChangeFilter.Get1(count - 1).NewStateName;
            _gameStateFilter.Get1(0).CurrentState = nextState;
            _stateMachine.ChangeState(nextState);
        }
    }
}
