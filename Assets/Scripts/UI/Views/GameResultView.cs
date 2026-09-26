using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fives.Domain;
using Scripts.Models;
using Scripts.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class GameResultView : View<GameResultPresenter>, IGameResultView
    {
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button getRewardButton;
        [SerializeField] private Button doubleRewardButton;
        [SerializeField] private TextMeshProUGUI starsRewardText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI levelProgressText;
        [SerializeField] private TextMeshProUGUI themeNameText;
        [SerializeField] private Slider themeSliderProgress;

        protected override void OnInitialized()
        {
            getRewardButton.onClick.AddListener(OnGetRewardClicked);
            doubleRewardButton.onClick.AddListener(OnGetDoubleRewardClicked);
        }

        public override UniTask PlayShowAnimation()
        {
            backgroundImage.DOFade(1, 0.5f).SetLink(gameObject);
            return Play(contentContainer.DOScaleY(1, 0.5f).SetEase(Ease.OutBack));
        }

        public override UniTask PlayHideAnimation()
        {
            return default;
        }

        public void UpdateViewContent(string stars, string stats, string themeName, ThemeProgress progress)
        {
            starsRewardText.text = stars;
            statsText.text = stats;
            levelProgressText.text = progress.ToString();
            themeNameText.text = themeName;
            var target = progress.Total == 0 ? 1f : (float)progress.Completed / progress.Total;
            themeSliderProgress
                .DOValue(target, 1f)
                .SetDelay(0.7f)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }
    
        private void OnGetRewardClicked()
        {
            Presenter.GetReward(false);
        }

        private void OnGetDoubleRewardClicked()
        {
            Presenter.GetReward(true);
        }
    }
}
