using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Helpers.StateMachine;
using Scripts.Models;

namespace Scripts.Systems
{
    public class GameStateSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsFilter<ChangeStateEvent> _stateChangeFilter;
        private readonly EcsFilter<GameStateComponent> _gameStateFilter;
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
            foreach (var i in _stateChangeFilter)
            {
                ref var stateChangeEvent = ref _stateChangeFilter.Get1(i);

                foreach (var j in _gameStateFilter)
                {
                    ref var gameState = ref _gameStateFilter.Get1(j);
                    gameState.CurrentState = stateChangeEvent.NewStateName;
                }

                _stateMachine.ChangeState(stateChangeEvent.NewStateName);
                _stateChangeFilter.GetEntity(i).Destroy();
            }
        }
    }
}
