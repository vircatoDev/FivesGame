using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts.Models;
using Scripts.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class MainMenuView : View<MainMenuPresenter>, IMainMenuView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button levelButton;
        [Header("Carousel: left, center, right")]
        [SerializeField] private PuzzleCardView[] cards;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;

        private const float SlideDuration = 0.35f;
        private readonly PuzzleCardView[] _slots = new PuzzleCardView[3];
        private (Vector2 position, Quaternion rotation, Vector3 scale)[] _poses;

        private Vector2 _initialPosition;
        private RectTransform _menuContainer;

        public void Awake()
        {
            _menuContainer = GetComponent<RectTransform>();
            _initialPosition = _menuContainer.anchoredPosition;
            // Slot poses are authored in the prefab: the cards' initial transforms.
            _poses = new (Vector2, Quaternion, Vector3)[cards.Length];
            for (var i = 0; i < cards.Length; i++)
            {
                _slots[i] = cards[i];
                _poses[i] = (cards[i].Rect.anchoredPosition, cards[i].Rect.localRotation, cards[i].Rect.localScale);
            }
        }

        public void OnEnable()
        {
            ResetPosition();
        }

        protected override void OnInitialized()
        {
            startGameButton.onClick.AddListener(Presenter.OnStartGame);
            levelButton.onClick.AddListener(Presenter.OnSelectMenu);
            previousButton.onClick.AddListener(Presenter.OnPreviousTheme);
            nextButton.onClick.AddListener(Presenter.OnNextTheme);
        }

        /// <summary>
        /// Shows the previous, current and next themes. A non-zero direction slides the cards one slot:
        /// +1 brings the right card to the center, -1 the left one; the card that wraps around passes behind.
        /// </summary>
        public void ShowThemes(in ThemeCard previous, in ThemeCard current, in ThemeCard next, int direction, bool canBrowse)
        {
            if (direction > 0)
                (_slots[0], _slots[1], _slots[2]) = (_slots[1], _slots[2], _slots[0]);
            else if (direction < 0)
                (_slots[0], _slots[1], _slots[2]) = (_slots[2], _slots[0], _slots[1]);

            _slots[0].Show(previous);
            _slots[1].Show(current);
            _slots[2].Show(next);
            _slots[1].Rect.SetAsLastSibling();

            previousButton.gameObject.SetActive(canBrowse);
            nextButton.gameObject.SetActive(canBrowse);

            for (var i = 0; i < _slots.Length; i++)
                MoveToSlot(_slots[i].Rect, _poses[i], direction != 0);
        }

        public bool IsSliding => DOTween.IsTweening(_slots[1].Rect);

        private void MoveToSlot(RectTransform card, (Vector2 position, Quaternion rotation, Vector3 scale) pose, bool animate)
        {
            card.DOKill();
            if (!animate)
            {
                card.anchoredPosition = pose.position;
                card.localRotation = pose.rotation;
                card.localScale = pose.scale;
                return;
            }

            card.DOAnchorPos(pose.position, SlideDuration).SetEase(Ease.OutCubic).SetLink(card.gameObject);
            card.DOLocalRotateQuaternion(pose.rotation, SlideDuration).SetEase(Ease.OutCubic).SetLink(card.gameObject);
            card.DOScale(pose.scale, SlideDuration).SetEase(Ease.OutBack).SetLink(card.gameObject);
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