using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class MainMenuView : View<MainMenuPresenter>
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button levelButton;
        [SerializeField] private TextMeshProUGUI previewThemeName;
        [SerializeField] private TextMeshProUGUI previewThemeProgress;
        [SerializeField] private Image previewThemeImage;


        private Vector2 _initialPosition;
        private RectTransform _menuContainer;

        public void Awake()
        {
            _menuContainer = GetComponent<RectTransform>();
            _initialPosition = _menuContainer.anchoredPosition;
        }

        public void OnEnable()
        {
            ResetPosition();
        }

        protected override void OnInitialized()
        {
            startGameButton.onClick.AddListener(Presenter.OnStartGame);
            levelButton.onClick.AddListener(Presenter.OnSelectMenu);
        }

        public void UpdateViewContent(string themeProgress, string themeName, Sprite themePreview)
        {
            previewThemeProgress.text = themeProgress;
            previewThemeName.text = themeName;
            previewThemeImage.sprite = themePreview;
        }

        public override async UniTask PlayShowAnimation()
        {
            canvasGroup.DOFade(0, 0f);
            canvasGroup.DOFade(1, 0.5f);
            await MoveScreenAnimation(0f, -1000f);
            await MoveScreenAnimation(0.5f, 1000f);
        }
        public override async UniTask PlayHideAnimation()
        {  
            canvasGroup.DOFade(0, 0.5f);
            await MoveScreenAnimation(0.5f, 1000f);
        }

        private void ResetPosition() => _menuContainer.anchoredPosition = _initialPosition;
        private async Task MoveScreenAnimation(float duration, float offset)
        {
            await _menuContainer
                .DOAnchorPosX(_menuContainer.anchoredPosition.x + offset, duration)
                .SetEase(Ease.InBack)
                .AsyncWaitForCompletion();
        }
    }
}