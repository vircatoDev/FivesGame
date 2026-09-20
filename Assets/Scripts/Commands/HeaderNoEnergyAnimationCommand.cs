using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Commands
{
    public class HeaderNoEnergyAnimationCommand : ICommand
    {
        private readonly EcsWorld _world;

        public HeaderNoEnergyAnimationCommand(EcsWorld world)
        {
            _world = world;
        }

        public void Execute()
        {
            var noEnergyAnimationEvent = _world.NewEntity();
            noEnergyAnimationEvent.Replace(new UpdateControlPanelEnergyEvent
            {
                EnergyNotEnough = true
            });
        }
    }
}