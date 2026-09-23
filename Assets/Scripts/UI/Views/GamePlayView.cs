using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts.Models;
using Scripts.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class GamePlayView : View<GamePlayPresenter>
    {
        [Header("UI Blocks")] [SerializeField] private RectTransform previewRectTransform;
        [SerializeField] private RectTransform infoRectTransform;

        [SerializeField] private Image puzzlePreviewImg;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI mainText;

        [SerializeField] private BoardControlsView controls;
    
        private Vector2 _initialLeftBlockPosition;
        private Vector2 _initialRightBlockPosition;

        private void Awake()
        {
            controls?.Initialize(control => Presenter?.RequestControl(control));
            _initialLeftBlockPosition = previewRectTransform.anchoredPosition;
            _initialRightBlockPosition = infoRectTransform.anchoredPosition;
        }

        public void UpdateControls(string status, bool canUndo, bool canReplay, bool replaying) =>
            controls?.Refresh(status, canUndo, canReplay, replaying);

        public void OnEnable()
        {
            ResetPositions();

            Presenter?.OnActivateView();
        }

        public override async UniTask PlayShowAnimation()
        {
            int offset = 500;
            float duration = 0.5f;

            var leftTargetPos = previewRectTransform.anchoredPosition.x;
            var rightTargetPos = infoRectTransform.anchoredPosition.x;

            previewRectTransform.anchoredPosition = new Vector2(-offset, previewRectTransform.anchoredPosition.y);
            infoRectTransform.anchoredPosition = new Vector2(offset, infoRectTransform.anchoredPosition.y);

            var leftTween = previewRectTransform.DOAnchorPosX(leftTargetPos, duration).SetEase(Ease.OutBack);
            var rightTween = infoRectTransform.DOAnchorPosX(rightTargetPos, duration).SetEase(Ease.OutBack);

            await UniTask.WhenAll(leftTween.AsyncWaitForCompletion().AsUniTask(),
                rightTween.AsyncWaitForCompletion().AsUniTask());
        }

        public override UniTask PlayHideAnimation()
        {
            return default;
        }

        public void UpdateViewContent(PuzzleData selectedPuzzle)
        {
            puzzlePreviewImg.sprite = selectedPuzzle.Image;
            titleText.text = selectedPuzzle.Name;
            mainText.text = selectedPuzzle.Description;
        }

        public void ResetPositions()
        {
            previewRectTransform.anchoredPosition = _initialLeftBlockPosition;
            infoRectTransform.anchoredPosition = _initialRightBlockPosition;
        }
    }
}