using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Commands
{
    public class HeaderNoStarsAnimationCommand : ICommand
    {
        private readonly EcsWorld _world;

        public HeaderNoStarsAnimationCommand(EcsWorld world)
        {
            _world = world;
        }

        public void Execute()
        {
            var noStarsAnimationEvent = _world.NewEntity();
            noStarsAnimationEvent.Replace(new UpdateControlPanelStarsEvent
            {
                StarsNotEnough = true
            });
        }
    }
}