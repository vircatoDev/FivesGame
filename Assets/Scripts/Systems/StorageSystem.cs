using System.Collections.Generic;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    public class StorageSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsFilter<SaveDataEvent> _saveEvents;
        private GameSaveData _gameSaveData;
        private readonly IEnumerable<IStorable> _storables;
        private readonly PlayerDataSaveHelper _playerDataSaveHelper;

        public void Init()
        {
            _gameSaveData = _playerDataSaveHelper.GetPlayerData() ?? new GameSaveData();
        }

        public void Run()
        {
            foreach (var i in _saveEvents)
            {
                var storableObject = _saveEvents.Get1(i).StorableObject;

                storableObject.UpdatePlayerData(_gameSaveData);

                _playerDataSaveHelper.SavePlayerData(_gameSaveData);
            }
        }
    }
}