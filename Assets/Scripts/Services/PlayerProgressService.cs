using System.Collections.Generic;
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

        public void UnlockTheme(string theme)
        {
            if (!_progressData.UnlockedThemes.Contains(theme))
            {
                _progressData.UnlockedThemes.Add(theme);
            }
        }

        public void MarkPuzzleCompleted(string puzzleId)
        {
            if (!_progressData.CompletedPuzzles.Contains(puzzleId))
            {
                _progressData.CompletedPuzzles.Add(puzzleId);
            }
        }

        public PlayerProgressData GetProgressData()
        {
            return _progressData;
        }

        public PlayerProgressData GetDataForSave()
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