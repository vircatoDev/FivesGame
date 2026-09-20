using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Commands
{
    public class EndGameCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly int _delay;

        public EndGameCommand(EcsWorld world, int delay)
        {
            _world = world;
            _delay = delay;
        }

        public void Execute()
        {
            var stateChangeEvent = _world.NewEntity();
            stateChangeEvent.Replace(new GameEndEvent
            {
                Delay = _delay
            });
        }
    }
}