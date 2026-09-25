using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using Scripts.Models;
using Scripts.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class SelectMenuView : View<SelectMenuPresenter>, ISelectMenuView
    {
        [Header("UI Elements")] [SerializeField]
        private RectTransform scrollView;

        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject puzzleItemPrefab; 
        [SerializeField] private RectTransform textContainer;
        [SerializeField] private SimpleScrollSnap scrollSnap; 

        private float animationDuration = 0.45f; 
        private Vector2 initialScrollViewPosition; 
        private Vector2 initialTextPosition; 
        private Vector2 scrollViewOffScreenPosition; 
        private Vector2 textOffScreenPosition; 

        private List<MenuItemView> _activeMenuItemViews = new List<MenuItemView>();

        protected override void OnInitialized()
        {
            exitButton.onClick.AddListener(OnExitClicked);
            initialScrollViewPosition = scrollView.anchoredPosition;
            initialTextPosition = textContainer.anchoredPosition;
        
            scrollViewOffScreenPosition = new Vector2(
                initialScrollViewPosition.x,
                initialScrollViewPosition.y - Screen.height
            );
            textOffScreenPosition = new Vector2(
                initialTextPosition.x,
                initialTextPosition.y + Screen.height
            );

            // Off screen right away: a zero-length tween would apply only on DOTween's next update, and the first
            // frame would show the prefab as authored, with its placeholder English title.
            scrollView.anchoredPosition = scrollViewOffScreenPosition;
            textContainer.anchoredPosition = textOffScreenPosition;
        }


        public override async UniTask PlayShowAnimation()
        {
            Sequence sequence = CreateShowSequence();
            await sequence.Play().AsyncWaitForCompletion();
        }

        public override async UniTask PlayHideAnimation()
        {
            Sequence sequence = CreateHideSequence();
            await sequence.Play().AsyncWaitForCompletion();
        }

        public void UpdateViewContent(MenuItemData[] newContent, string titleText, Action<string> onClick,
            bool playAnimation, int centeredItem = 0)
        {
            Sequence sequence = CreateUpdateSequence(playAnimation);

            sequence.AppendCallback(() =>
            {
                UpdateScrollViewContent(newContent, onClick);
                if (centeredItem > 0)
                    scrollSnap.GoToPanel(centeredItem);
                // Two-word titles sit on the plate: the first word on the cloud, the second on the plank.
                textContainer.GetComponentInChildren<TextMeshProUGUI>().text = titleText.Replace(' ', '\n');
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
                .Append(scrollView.DOAnchorPos(initialScrollViewPosition, animationDuration).SetEase(Ease.InOutQuad))
                .Join(textContainer.DOAnchorPos(initialTextPosition, animationDuration).SetEase(Ease.InOutQuad));
        }

        private Sequence CreateHideSequence()
        {
            return DOTween.Sequence()
                .Append(scrollView.DOAnchorPos(scrollViewOffScreenPosition, animationDuration).SetEase(Ease.InOutQuad))
                .Join(textContainer.DOAnchorPos(textOffScreenPosition, animationDuration).SetEase(Ease.InOutQuad));
        }

        private Sequence CreateUpdateSequence(bool playAnimation)
        {
            return DOTween.Sequence()
                .Append(textContainer.DOAnchorPos(textOffScreenPosition, playAnimation ? animationDuration : 0)
                    .SetEase(Ease.InOutQuad))
                .Join(scrollView.DOAnchorPos(scrollViewOffScreenPosition, playAnimation ? animationDuration : 0)
                    .SetEase(Ease.InOutQuad));
        }

        private Sequence CreateRestoreSequence()
        {
            return DOTween.Sequence()
                .Append(scrollView.DOAnchorPos(initialScrollViewPosition, animationDuration).SetEase(Ease.InOutQuad))
                .Join(textContainer.DOAnchorPos(initialTextPosition, animationDuration).SetEase(Ease.InOutQuad));
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