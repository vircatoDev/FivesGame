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

        private readonly IStorageService _storage;
        private GameSaveData _gameSaveData;

        public PlayerDataSaveHelper(IStorageService storage, GlobalConfig gameSettings)
        {
            _storage = storage;
            _gameSaveData = _storage.Load<GameSaveData>(SAVE_KEY)
                ?? new GameSaveData { Stars = gameSettings.InitialStars };
            NormalizePlayerData(gameSettings);
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

        private void NormalizePlayerData(GlobalConfig gameSettings)
        {
            _gameSaveData.Energy ??= new EnergyData
            {
                CurrentEnergy = gameSettings.InitialEnergy,
                LastRecoveryTime = DateTime.UtcNow
            };

            if (_gameSaveData.Energy.LastRecoveryTime == default)
                _gameSaveData.Energy.LastRecoveryTime = DateTime.UtcNow;

            _gameSaveData.Energy.CurrentEnergy = Math.Clamp(_gameSaveData.Energy.CurrentEnergy, 0, gameSettings.MaxEnergy);
            _gameSaveData.Stars = Math.Max(0, _gameSaveData.Stars);
            _gameSaveData.SoundSettings ??= new SoundSettingsData();
            _gameSaveData.PlayerProgress ??= new PlayerProgressData();
            var progress = _gameSaveData.PlayerProgress;
            progress.UnlockedThemes = gameSettings.DefaultUnlockedThemes
                .Concat(progress.UnlockedThemes ?? Enumerable.Empty<string>())
                .Where(name => !string.IsNullOrEmpty(name)).Distinct().ToList();
            progress.CompletedPuzzles ??= new List<string>();
        }
    }
}
