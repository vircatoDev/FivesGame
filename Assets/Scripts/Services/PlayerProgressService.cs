using System.Collections.Generic;
using System.Linq;
using Fives.Domain;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;

namespace Scripts.Services
{
    public class PlayerProgressService : IStorable
    {
        private PlayerProgressData _progressData;

        public PlayerProgressService(PlayerDataSaveHelper saveHelper)
        {
            _progressData = new PlayerProgressData
            {
                UnlockedThemes = new List<string>(),
                CompletedPuzzles = new List<string>()
            };

            SetDataFromSave(saveHelper.GetPlayerData().PlayerProgress);
        }

        public bool IsUnlocked(ThemeConfig theme) => _progressData.UnlockedThemes.Contains(theme.Id);

        public bool IsCompleted(PuzzleData puzzle) => _progressData.CompletedPuzzles.Contains(puzzle.Id);

        public void Unlock(ThemeConfig theme)
        {
            if (!IsUnlocked(theme))
                _progressData.UnlockedThemes.Add(theme.Id);
        }

        public void MarkCompleted(PuzzleData puzzle)
        {
            if (!IsCompleted(puzzle))
                _progressData.CompletedPuzzles.Add(puzzle.Id);
        }

        public ThemeProgress GetThemeProgress(ThemeConfig theme) =>
            new ThemeProgress(theme.Puzzles.Select(p => p.Id).ToArray(), _progressData.CompletedPuzzles);

        public PlayerProgressData GetProgressData()
        {
            return _progressData;
        }

        public void SetDataFromSave(PlayerProgressData data)
        {
            _progressData = data;
        }

        public void UpdatePlayerData(GameSaveData playerData)
        {
            playerData.PlayerProgress.UnlockedThemes = _progressData.UnlockedThemes;
            playerData.PlayerProgress.CompletedPuzzles = _progressData.CompletedPuzzles;
        }
    }

    [System.Serializable]
    public class PlayerProgressData
    {
        public List<string> UnlockedThemes;
        public List<string> CompletedPuzzles;
    }
}