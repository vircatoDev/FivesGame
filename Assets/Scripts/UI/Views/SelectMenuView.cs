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
    public class SelectMenuView : BaseView
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
        private SelectMenuPresenter _presenter;

        public override void Initialize(BasePresenter initData)
        {
            if (initData is not SelectMenuPresenter presenter)
            {
                Debug.LogError("SelectMenuView: wrong initData");
                return;
            }

            SetupView();

            _presenter = presenter;
            _presenter.Initialize(this);
        }

        private void SetupView()
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

            CreateUpdateSequence(false);
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
            bool playAnimation)
        {
            Sequence sequence = CreateUpdateSequence(playAnimation);

            sequence.AppendCallback(() =>
            {
                UpdateScrollViewContent(newContent, onClick);
                textContainer.GetComponentInChildren<TextMeshProUGUI>().text = titleText;
            });

            sequence.Append(CreateRestoreSequence());
            sequence.Play();
        }

        public void UnlockThemeItemByName(MenuItemData itemData, Action<string> onTileClick)
        {
            var menuItem = _activeMenuItemViews.FirstOrDefault(x => x.GetId() == itemData.Id);
            menuItem?.Initialize(itemData, onTileClick, _presenter.OnThemeBuy);
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
                menuItem.Initialize(tile, onClick, _presenter.OnThemeBuy);
                _activeMenuItemViews.Add(menuItem);
            }
        }

        private void OnExitClicked()
        {
            _presenter.OnExit();
        }
    }
}