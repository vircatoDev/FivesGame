using System;
using Leopotam.Ecs;

namespace Scripts.Commands
{
    public class RaiseEventCommand<TEvent> : ICommand where TEvent : struct
    {
        private readonly EcsWorld _world;

        public RaiseEventCommand(EcsWorld world)
        {
            _world = world;
        }

        public void Execute()
        {
            var entity = _world.NewEntity();
            var eventInstance = Activator.CreateInstance<TEvent>();
            entity.Get<TEvent>() = eventInstance;
        }
    }
}