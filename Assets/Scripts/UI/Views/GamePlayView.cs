using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts.Models;
using Scripts.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class GamePlayView : View<GamePlayPresenter>, IGamePlayView
    {
        [Header("UI Blocks")] [SerializeField] private RectTransform previewRectTransform;
        [SerializeField] private RectTransform infoRectTransform;

        [SerializeField] private Image puzzlePreviewImg;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI mainText;

        [SerializeField] private BoardControlsView controls;
    
        private Vector2 _initialPreviewPosition;
        private Vector2 _initialInfoPosition;

        private void Awake()
        {
            controls?.Initialize(control => Presenter?.RequestControl(control));
            _initialPreviewPosition = previewRectTransform.anchoredPosition;
            _initialInfoPosition = infoRectTransform.anchoredPosition;
        }

        public void UpdateControls(string moves, string hintPrice, bool canUndo, bool canHint) =>
            controls?.Refresh(moves, hintPrice, canUndo, canHint);

        public void OnEnable()
        {
            ResetPositions();

            Presenter?.OnActivateView();
        }

        public override UniTask PlayShowAnimation() =>
            UniTask.WhenAll(SlideInFromRight(previewRectTransform), SlideInFromRight(infoRectTransform));

        private UniTask SlideInFromRight(RectTransform block)
        {
            const float offset = 500f;
            const float duration = 0.5f;

            var target = block.anchoredPosition.x;
            block.anchoredPosition += Vector2.right * offset;
            return Play(block.DOAnchorPosX(target, duration).SetEase(Ease.OutBack));
        }

        public override UniTask PlayHideAnimation()
        {
            return default;
        }

        public void UpdateViewContent(Sprite image, string title, string about)
        {
            puzzlePreviewImg.SetCover(image);
            titleText.text = title;
            mainText.text = about;
        }

        public void ResetPositions()
        {
            previewRectTransform.anchoredPosition = _initialPreviewPosition;
            infoRectTransform.anchoredPosition = _initialInfoPosition;
        }
    }
}