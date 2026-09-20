using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Services;

namespace Scripts.Commands
{
    public class UpdateStarBalanceCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly StarService _starService;
        private readonly int _amount;

        public UpdateStarBalanceCommand(EcsWorld world, StarService starService, int amount)
        {
            _world = world;
            _starService = starService;
            _amount = amount;
        }

        public void Execute()
        {
            var updateControlPanelEvent = _world.NewEntity();
            updateControlPanelEvent.Replace(new UpdateControlPanelStarsEvent
            {
                StarsAmount = _starService.GetBalance(),
                StarsChange = _amount
            });
        }
    }
}