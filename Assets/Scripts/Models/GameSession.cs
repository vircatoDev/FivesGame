using Fives.Domain;
using Scripts.Configs;
using UnityEngine;

namespace Scripts.Models
{
    public class GameSession {
        private readonly RewardClaim _rewardClaim = new RewardClaim();
    
        public ThemeConfig SelectedTheme { get; private set; }
        public PuzzleData SelectedPuzzle { get; private set; }
        public GameSettings SelectedGameMode { get; private set; }
        public GameResult LastGameResult   { get; private set; }
        public bool IsTimedMode { get; private set; }
        public float RemainingTime { get; private set; }

    
        public void SetSelectedImage(PuzzleData puzzle) {
            SelectedPuzzle = puzzle;
        }

        public void BeginRun()
        {
            _rewardClaim.Reset();
            LastGameResult = new GameResult { StarCount = 10 };
        }

        public bool TryClaimReward(bool doubleReward, out int grantedAmount)
        {
            return _rewardClaim.TryClaim(10, doubleReward, out grantedAmount);
        }
    
        public void SetSelectedTheme(ThemeConfig theme) {
            SelectedTheme = theme;
        }

        public void SetGameMode(GameSettings gameMode, bool isTimed) {
            SelectedGameMode = gameMode;
            IsTimedMode = isTimed;
        }

        public void UpdateRemainingTime(float deltaTime) {
            if (IsTimedMode) {
                RemainingTime = Mathf.Max(0, RemainingTime - deltaTime);
            }
        }

        public void ResetSession() {
            SelectedPuzzle = null;
            SelectedGameMode = null;
            LastGameResult = null;
            _rewardClaim.Reset();
            IsTimedMode = false;
            RemainingTime = 0;
        }
    }
}
