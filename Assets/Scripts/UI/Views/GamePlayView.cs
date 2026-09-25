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

        public void UpdateControls(string moves, bool canUndo, bool canRedo) =>
            controls?.Refresh(moves, canUndo, canRedo);

        public void OnEnable()
        {
            ResetPositions();

            Presenter?.OnActivateView();
        }

        public override UniTask PlayShowAnimation() =>
            UniTask.WhenAll(SlideInFromRight(previewRectTransform), SlideInFromRight(infoRectTransform));

        private static UniTask SlideInFromRight(RectTransform block)
        {
            const float offset = 500f;
            const float duration = 0.5f;

            var target = block.anchoredPosition.x;
            block.anchoredPosition += Vector2.right * offset;
            return block.DOAnchorPosX(target, duration).SetEase(Ease.OutBack).AsyncWaitForCompletion().AsUniTask();
        }

        public override UniTask PlayHideAnimation()
        {
            return default;
        }

        public void UpdateViewContent(PuzzleData selectedPuzzle)
        {
            puzzlePreviewImg.SetCover(selectedPuzzle.Image);
            titleText.text = selectedPuzzle.Name;
            mainText.text = selectedPuzzle.Description;
        }

        public void ResetPositions()
        {
            previewRectTransform.anchoredPosition = _initialPreviewPosition;
            infoRectTransform.anchoredPosition = _initialInfoPosition;
        }
    }
}