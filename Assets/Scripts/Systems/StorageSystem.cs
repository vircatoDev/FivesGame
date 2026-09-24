using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Helpers;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    /// <summary>Writes every service's data in one save when any save was requested this frame.</summary>
    public class StorageSystem : IEcsRunSystem
    {
        private readonly EcsFilter<SaveDataEvent> _saveEvents = null;
        private readonly PlayerDataSaveHelper _playerDataSaveHelper = null;
        private readonly IStorable[] _storables;

        public StorageSystem(params IStorable[] storables)
        {
            _storables = storables;
        }

        public void Run()
        {
            if (_saveEvents.GetEntitiesCount() > 0)
                _playerDataSaveHelper.SaveAll(_storables);
        }
    }
}
