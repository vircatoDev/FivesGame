using System;
using System.Collections.Generic;
using System.Linq;
using Fives.Domain;
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
                ?? new GameSaveData { Version = ProgressMigration.CurrentVersion, Stars = gameSettings.InitialStars };
            Migrate(gameSettings);
            NormalizePlayerData(gameSettings);
        }

        public GameSaveData GetPlayerData()
        {
            return _gameSaveData;
        }

        /// <summary>Writes the current state of every service in one save.</summary>
        public void SaveAll(params IStorable[] storables)
        {
            foreach (var storable in storables)
                storable.UpdatePlayerData(_gameSaveData);
            Save();
        }

        private void Save()
        {
            _storage.Save(SAVE_KEY, _gameSaveData);
        }

        private void Migrate(GlobalConfig gameSettings)
        {
            if (_gameSaveData.Version < 1 && _gameSaveData.PlayerProgress != null)
            {
                var progress = _gameSaveData.PlayerProgress;
                progress.UnlockedThemes = ProgressMigration.NamesToIds(progress.UnlockedThemes ?? new List<string>(),
                    gameSettings.Themes.Select(theme => (theme.ThemeName, theme.Id)));
                progress.CompletedPuzzles = ProgressMigration.NamesToIds(progress.CompletedPuzzles ?? new List<string>(),
                    gameSettings.Themes.SelectMany(theme => theme.Puzzles).Select(puzzle => (puzzle.Name, puzzle.Id)));
            }

            _gameSaveData.Version = ProgressMigration.CurrentVersion;
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
