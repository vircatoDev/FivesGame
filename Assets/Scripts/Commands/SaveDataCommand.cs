using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Services.Interfaces;

namespace Scripts.Commands
{
    public class SaveDataCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly IStorable _storable;

        public SaveDataCommand(EcsWorld world, IStorable storable)
        {
            _world = world;
            _storable = storable;
        }

        public void Execute()
        {
            var saveDataEvent = _world.NewEntity();
            saveDataEvent.Replace(new SaveDataEvent
            {
                StorableObject = _storable
            });
        }
    }
}