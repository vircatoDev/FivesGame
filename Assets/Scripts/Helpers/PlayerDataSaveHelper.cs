using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.Services.Interfaces;

namespace Scripts.Helpers
{
    public class PlayerDataSaveHelper
    {
        private const string SAVE_KEY = "GameSaveData";

        private IStorageService _storage;
        private GameSaveData _gameSaveData;

        public PlayerDataSaveHelper(IStorageService storage, GlobalConfig gameSettings)
        {
            _storage = storage;
            _gameSaveData = _storage.Load<GameSaveData>(SAVE_KEY, new GameSaveData());
            if (_gameSaveData.PlayerProgress == null)
            {
                InitPlayerData(gameSettings);
            }
        }

        public GameSaveData GetPlayerData()
        {
            return _gameSaveData;
        }

        public void SavePlayerData(GameSaveData saveData)
        {
            _gameSaveData = saveData;
            Save();
        }

        private void Save()
        {
            _storage.Save(SAVE_KEY, _gameSaveData);
        }

        private void InitPlayerData(GlobalConfig gameSettings)
        {
            _gameSaveData.Energy = new EnergyData
            {
                CurrentEnergy = gameSettings.InitialEnergy,
                LastRecoveryTime = DateTime.Now
            };

            _gameSaveData.PlayerProgress = new PlayerProgressData
            {
                UnlockedThemes = gameSettings.DefaultUnlockedThemes.ToList(),
                CompletedPuzzles = new List<string>()
            };

            _gameSaveData.Stars = gameSettings.InitialStars;
        }
    }
}