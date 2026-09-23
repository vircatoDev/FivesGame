using Fives.Domain;
using Scripts.Configs;

namespace Scripts.Models
{
    public class GameSession
    {
        private readonly RewardClaim _rewardClaim = new RewardClaim();
        private readonly int _rewardStars;

        public ThemeConfig SelectedTheme { get; private set; }
        public PuzzleData SelectedPuzzle { get; private set; }
        public GameSettings SelectedGameMode { get; private set; }
        public GameResult LastGameResult { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsCompleted { get; private set; }

        public GameSession(GlobalConfig config)
        {
            _rewardStars = config.RewardStars;
        }

        public void SetSelectedImage(PuzzleData puzzle)
        {
            SelectedPuzzle = puzzle;
        }

        public void BeginRun()
        {
            IsRunning = true;
            IsCompleted = false;
            _rewardClaim.Reset();
            LastGameResult = new GameResult { StarCount = _rewardStars };
        }

        public void CompleteRun()
        {
            IsCompleted = true;
        }

        public void EndRun()
        {
            IsRunning = false;
        }

        public bool TryClaimReward(bool doubleReward, out int grantedAmount)
        {
            return _rewardClaim.TryClaim(_rewardStars, doubleReward, out grantedAmount);
        }

        public void SetSelectedTheme(ThemeConfig theme)
        {
            SelectedTheme = theme;
        }

        public void SetGameMode(GameSettings gameMode)
        {
            SelectedGameMode = gameMode;
        }
    }
}
