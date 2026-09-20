using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Systems
{
    class GamePlayManagementSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsSystems _systems = null;
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;
    
        private bool _boardWasCreated;
        private readonly int _idx;

        public GamePlayManagementSystem(EcsSystems systems)
        {
            _systems = systems;
            _idx = systems.GetNamedRunSystem ("gamePlay");
        }

        public void Run()
        {
            var _cachedState = _stateFilter.Get1(0);

            if (_cachedState.CurrentState == GameStateType.Playing )
            {
                if (!_boardWasCreated)
                {
                    _systems.SetRunSystemState(_idx, true);
                    _boardWasCreated = true;
                }
            }
            else
            {
                _systems.SetRunSystemState (_idx, false);
                _boardWasCreated = false;
            }
        }
    }
}