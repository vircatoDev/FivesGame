using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DanielLochner.Assets.SimpleScrollSnap;
using Fives.Models;
using Fives.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fives.UI.Views
{
    public class SelectMenuView : View<SelectMenuPresenter>, ISelectMenuView
    {
        [Header("UI Elements")] [SerializeField]
        private RectTransform scrollView;

        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject puzzleItemPrefab;
        [SerializeField] private RectTransform textContainer;
        [SerializeField] private SimpleScrollSnap scrollSnap;

        private const float AnimationDuration = 0.45f;

        private Vector2 _initialScrollViewPosition;
        private Vector2 _initialTextPosition;
        private Vector2 _scrollViewOffScreenPosition;
        private Vector2 _textOffScreenPosition;

        private List<MenuItemView> _activeMenuItemViews = new List<MenuItemView>();

        protected override void OnInitialized()
        {
            exitButton.onClick.AddListener(OnExitClicked);
            _initialScrollViewPosition = scrollView.anchoredPosition;
            _initialTextPosition = textContainer.anchoredPosition;
        
            _scrollViewOffScreenPosition = new Vector2(
                _initialScrollViewPosition.x,
                _initialScrollViewPosition.y - Screen.height
            );
            _textOffScreenPosition = new Vector2(
                _initialTextPosition.x,
                _initialTextPosition.y + Screen.height
            );

            // Off screen right away: a zero-length tween would apply only on DOTween's next update, and the first
            // frame would show the prefab as authored, with its placeholder English title.
            scrollView.anchoredPosition = _scrollViewOffScreenPosition;
            textContainer.anchoredPosition = _textOffScreenPosition;
        }


        public override UniTask PlayShowAnimation() => Play(CreateShowSequence());

        public override UniTask PlayHideAnimation() => Play(CreateHideSequence());

        public void UpdateViewContent(MenuItemData[] newContent, string titleText, Action<string> onClick,
            bool playAnimation, int centeredItem = 0)
        {
            Sequence sequence = CreateUpdateSequence(playAnimation);

            sequence.AppendCallback(() =>
            {
                UpdateScrollViewContent(newContent, onClick);
                if (centeredItem > 0)
                    scrollSnap.GoToPanel(centeredItem);
                textContainer.GetComponentInChildren<TextMeshProUGUI>().text = TwoLineTitle.Split(titleText);
            });

            sequence.Append(CreateRestoreSequence());
            sequence.Play();
        }

        public void UnlockThemeItemByName(MenuItemData itemData, Action<string> onTileClick)
        {
            var menuItem = _activeMenuItemViews.FirstOrDefault(x => x.GetId() == itemData.Id);
            menuItem?.Initialize(itemData, onTileClick, Presenter.OnThemeBuy);
        }

        private Sequence CreateShowSequence()
        {
            return DOTween.Sequence()
                .Append(scrollView.DOAnchorPos(_initialScrollViewPosition, AnimationDuration).SetEase(Ease.InOutQuad))
                .Join(textContainer.DOAnchorPos(_initialTextPosition, AnimationDuration).SetEase(Ease.InOutQuad));
        }

        private Sequence CreateHideSequence()
        {
            return DOTween.Sequence()
                .Append(scrollView.DOAnchorPos(_scrollViewOffScreenPosition, AnimationDuration).SetEase(Ease.InOutQuad))
                .Join(textContainer.DOAnchorPos(_textOffScreenPosition, AnimationDuration).SetEase(Ease.InOutQuad));
        }

        private Sequence CreateUpdateSequence(bool playAnimation)
        {
            return DOTween.Sequence()
                .Append(textContainer.DOAnchorPos(_textOffScreenPosition, playAnimation ? AnimationDuration : 0)
                    .SetEase(Ease.InOutQuad))
                .Join(scrollView.DOAnchorPos(_scrollViewOffScreenPosition, playAnimation ? AnimationDuration : 0)
                    .SetEase(Ease.InOutQuad));
        }

        private Sequence CreateRestoreSequence()
        {
            return DOTween.Sequence()
                .Append(scrollView.DOAnchorPos(_initialScrollViewPosition, AnimationDuration).SetEase(Ease.InOutQuad))
                .Join(textContainer.DOAnchorPos(_initialTextPosition, AnimationDuration).SetEase(Ease.InOutQuad));
        }

        private void UpdateScrollViewContent(MenuItemData[] tiles, Action<string> onClick)
        {
            ClearScrollView();
            PopulateScrollView(tiles, onClick);
        }

        private void ClearScrollView()
        {
            for (int i = scrollSnap.NumberOfPanels - 1; i >= 0; i--)
            {
                scrollSnap.Remove(i);
            }
        }

        private void PopulateScrollView(MenuItemData[] tiles, Action<string> onClick)
        {
            _activeMenuItemViews.Clear();

            foreach (var tile in tiles)
            {
                scrollSnap.Add(puzzleItemPrefab, _activeMenuItemViews.Count);
                var menuItem = scrollSnap.Panels[_activeMenuItemViews.Count].GetComponent<MenuItemView>();
                menuItem.Initialize(tile, onClick, Presenter.OnThemeBuy);
                _activeMenuItemViews.Add(menuItem);
            }
        }

        private void OnExitClicked()
        {
            Presenter.OnExit();
        }
    }
}