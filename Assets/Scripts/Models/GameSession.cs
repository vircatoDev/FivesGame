using Scripts.Configs;
using UnityEngine;

namespace Scripts.Models
{
    public class GameSession {
    
        public ThemeConfig SelectedTheme { get; private set; }
        public PuzzleData SelectedPuzzle { get; private set; }
        public GameSettings SelectedGameMode { get; private set; }
        public GameResult LastGameResult   { get; private set; }
        public bool IsTimedMode { get; private set; }
        public float RemainingTime { get; private set; }

    
        public void SetSelectedImage(PuzzleData puzzle) {
            SelectedPuzzle = puzzle;
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
            IsTimedMode = false;
            RemainingTime = 0;
        }
    }
}