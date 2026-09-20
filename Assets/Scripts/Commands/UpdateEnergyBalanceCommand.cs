using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Services;

namespace Scripts.Commands
{
    public class UpdateEnergyBalanceCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly EnergyService _energyService;
        private readonly int _amount;

        public UpdateEnergyBalanceCommand(EcsWorld world, EnergyService energyService, int amount)
        {
            _world = world;
            _energyService = energyService;
            _amount = amount;
        }

        public void Execute()
        {
            var updateControlPanelEvent = _world.NewEntity();
            updateControlPanelEvent.Replace(new UpdateControlPanelEnergyEvent
            {
                EnergyAmount = _energyService.GetBalance(),
                EnergyChange = _amount
            });
        }
    }
}