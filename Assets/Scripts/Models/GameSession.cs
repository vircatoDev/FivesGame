using System;
using Fives.Domain;
using Scripts.Configs;
using UnityEngine;

namespace Scripts.Models
{
    /// <summary>
    /// What the player picked and how the last run went, for the screens and the board setup.
    /// Whether a run is in progress is not stored here: the board entity in the ECS world is the run.
    /// </summary>
    public class GameSession
    {
        private readonly RewardClaim _rewardClaim = new RewardClaim();
        private readonly int _rewardStars;

        public ThemeConfig SelectedTheme { get; private set; }
        public PuzzleData SelectedPuzzle { get; private set; }
        /// <summary>The loaded picture of <see cref="SelectedPuzzle"/>.</summary>
        public Sprite PuzzleImage { get; private set; }
        public GameSettings SelectedGameMode { get; private set; }
        public GameResult LastGameResult { get; private set; }

        public GameSession(GameBalance balance)
        {
            _rewardStars = balance.RewardStars;
        }

        public void SetSelectedImage(PuzzleData puzzle, Sprite image)
        {
            SelectedPuzzle = puzzle;
            PuzzleImage = image;
        }

        /// <summary>A new result to fill and a new reward to claim.</summary>
        public void BeginRun()
        {
            _rewardClaim.Reset();
            LastGameResult = new GameResult { StarCount = _rewardStars };
        }

        public void CompleteRun(int turnCount, TimeSpan gameTime)
        {
            LastGameResult.TurnCount = turnCount;
            LastGameResult.GameTime = gameTime;
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
