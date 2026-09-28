using Fives.Components;
using Fives.Services;
using Leopotam.Ecs;

namespace Fives.Systems
{
    /// <summary>Writes every service's data in one save when any save was requested this frame.</summary>
    public class StorageSystem : IEcsRunSystem
    {
        private readonly EcsFilter<SaveDataEvent> _saveEvents = null;
        private readonly PlayerSave _playerSave = null;
        private readonly IStorable[] _storables;

        public StorageSystem(params IStorable[] storables)
        {
            _storables = storables;
        }

        public void Run()
        {
            if (_saveEvents.GetEntitiesCount() > 0)
                _playerSave.SaveAll(_storables);
        }
    }
}
