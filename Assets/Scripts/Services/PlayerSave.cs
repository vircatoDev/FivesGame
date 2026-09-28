using System;
using System.Collections.Generic;
using System.Linq;
using Fives.Configs;
using Fives.Domain;
using Fives.Models;

namespace Fives.Services
{
    /// <summary>
    /// The player's save. <see cref="Fives.Boot.BootFlow"/> loads it once the balance is loaded, because a new save
    /// takes its starting stars and energy from the balance. Reading it earlier throws instead of returning a save
    /// started without Remote Config.
    /// </summary>
    public class PlayerSave
    {
        private const string SaveKey = "GameSaveData";

        private readonly IStorageService _storage;
        private readonly GlobalConfig _gameSettings;
        private readonly GameBalance _balance;
        private GameSaveData _gameSaveData;

        public PlayerSave(IStorageService storage, GlobalConfig gameSettings, GameBalance balance)
        {
            _storage = storage;
            _gameSettings = gameSettings;
            _balance = balance;
        }

        /// <summary>Reads the save, or starts a new one from the balance, and brings an old one up to date.</summary>
        public void Load()
        {
            _gameSaveData = _storage.Load<GameSaveData>(SaveKey)
                ?? new GameSaveData { Version = ProgressMigration.CurrentVersion, Stars = _balance.InitialStars };
            Migrate(_gameSettings);
            NormalizePlayerData(_gameSettings, _balance);
        }

        public GameSaveData GetPlayerData()
        {
            return _gameSaveData ?? throw new InvalidOperationException(
                "The save is not loaded: BootFlow loads it after the balance, and nothing may read it before.");
        }

        /// <summary>Writes the current state of every service in one save.</summary>
        public void SaveAll(params IStorable[] storables)
        {
            var data = GetPlayerData();
            foreach (var storable in storables)
                storable.UpdatePlayerData(data);
            Save();
        }

        private void Save()
        {
            _storage.Save(SaveKey, _gameSaveData);
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

        private void NormalizePlayerData(GlobalConfig gameSettings, GameBalance balance)
        {
            _gameSaveData.Energy ??= new EnergyData
            {
                CurrentEnergy = balance.InitialEnergy,
                LastRecoveryTime = DateTime.UtcNow
            };

            if (_gameSaveData.Energy.LastRecoveryTime == default)
                _gameSaveData.Energy.LastRecoveryTime = DateTime.UtcNow;

            _gameSaveData.Energy.CurrentEnergy = Math.Clamp(_gameSaveData.Energy.CurrentEnergy, 0, balance.MaxEnergy);
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
