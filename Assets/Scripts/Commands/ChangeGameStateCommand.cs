using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Commands
{
    public class ChangeGameStateCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly GameStateType _newStateName;

        public ChangeGameStateCommand(EcsWorld world, GameStateType newStateName)
        {
            _world = world;
            _newStateName = newStateName;
        }

        public void Execute()
        {
            var stateChangeEvent = _world.NewEntity();
            stateChangeEvent.Replace(new ChangeStateEvent
            {
                NewStateName = _newStateName
            });
        }
    }
}