using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts.Configs;
using Scripts.Models;
using Scripts.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class GameResultView : BaseView
    {
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button getRewardButton;
        [SerializeField] private Button doubleRewardButton;
        [SerializeField] private TextMeshProUGUI starsRewardText;
        [SerializeField] private TextMeshProUGUI levelProgressText;
        [SerializeField] private TextMeshProUGUI themeNameText;
        [SerializeField] private Slider themeSliderProgress;

        private GameResultPresenter _presenter;

        public override void Initialize(BasePresenter initData)
        {
            if (initData is GameResultPresenter presenter)
            {
                _presenter = presenter;
                _presenter.Initialize(this);
                getRewardButton.onClick.AddListener(OnGetRewardClicked);
                doubleRewardButton.onClick.AddListener(OnGetDoubleRewardClicked);
            }
            else
            {
                Debug.LogError("GameResultView: wrong initData");
            }
        }

        public override async UniTask PlayShowAnimation()
        {
            backgroundImage.DOFade(1, 0.5f);
            await contentContainer.DOScaleY(1, 0.5f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        }

        public override UniTask PlayHideAnimation()
        {
            return default;
        }

        public void UpdateViewContent(GameResult gameSessionLastGameResult, ThemeConfig selectedTheme, PuzzleData selectedPuzzle)
        {
            var puzzleIndex = Array.IndexOf(selectedTheme.Puzzles, selectedPuzzle);
            var puzzlesCount = selectedTheme.Puzzles.Length;

            if (gameSessionLastGameResult == null)
                gameSessionLastGameResult = new GameResult();

            gameSessionLastGameResult.StarCount += 10;
            puzzleIndex += 1;
        
            starsRewardText.text = $"+{gameSessionLastGameResult.StarCount} Stars";
            levelProgressText.text = $"{puzzleIndex}/{puzzlesCount}";
            themeNameText.text = selectedTheme.ThemeName;
            var target = (float)puzzleIndex / (float)puzzlesCount;
            themeSliderProgress.DOValue(target, 1f).SetDelay(0.7f);
        }
    
        private void OnGetRewardClicked()
        {
            _presenter.GetReward(false);
        }

        private void OnGetDoubleRewardClicked()
        {
            _presenter.GetReward(true);
        }
    }
}