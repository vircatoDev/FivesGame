using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Systems
{
    /// <summary>Runs before the "gamePlay" group and enables it only while the game is in the Playing state.</summary>
    class GamePlayManagementSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsSystems _systems;
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;
        private int _idx;

        public GamePlayManagementSystem(EcsSystems systems)
        {
            _systems = systems;
        }

        public void Init() => _idx = _systems.GetNamedRunSystem("gamePlay");

        public void Run() =>
            _systems.SetRunSystemState(_idx, _stateFilter.Get1(0).CurrentState == GameStateType.Playing);
    }
}
