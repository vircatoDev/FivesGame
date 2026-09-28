using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fives.Components;
using Fives.Domain;
using Fives.Models;
using Fives.UI.Presenters;
using Fives.UI.Views;
using UnityEngine;

namespace Fives.Runtime.Tests
{
    /// <summary>A screen without Unity objects: shows at once, hides when the test completes <see cref="Hide"/>.</summary>
    internal class FakeView : IView
    {
        private readonly CancellationTokenSource _screen = new CancellationTokenSource();

        public UniTaskCompletionSource Hide = new UniTaskCompletionSource();
        public CancellationToken Lifetime => _screen.Token;
        public UniTask PlayShowAnimation() => UniTask.CompletedTask;
        // Like a real screen: destroying it cancels the animation instead of completing it.
        public UniTask PlayHideAnimation() => Hide.Task.AttachExternalCancellation(Lifetime);

        /// <summary>What destroying the screen does: its lifetime ends, then the presenter lets it go.</summary>
        public void Destroy(BasePresenter presenter)
        {
            _screen.Cancel();
            presenter.Deactivate();
        }
    }

    internal sealed class FakeMainMenuView : FakeView, IMainMenuView
    {
        public ThemeCard Previous, Current, Next;
        public int Direction;
        public bool CanBrowse;
        public bool IsSliding => false;

        public void ShowThemes(in ThemeCard previous, in ThemeCard current, in ThemeCard next, int direction, bool canBrowse)
        {
            Previous = previous; Current = current; Next = next; Direction = direction; CanBrowse = canBrowse;
        }
    }

    internal sealed class FakeSelectMenuView : FakeView, ISelectMenuView
    {
        public MenuItemData[] Items;
        /// <summary>What a tap on a card does: the theme or puzzle id goes in.</summary>
        public Action<string> OnClick;
        public MenuItemData Unlocked;

        public void UpdateViewContent(MenuItemData[] newContent, string titleText, Action<string> onClick, bool playAnimation, int centeredItem = 0)
        {
            Items = newContent;
            OnClick = onClick;
        }

        public void UnlockThemeItemByName(MenuItemData itemData, Action<string> onTileClick) => Unlocked = itemData;
    }

    internal sealed class FakeGamePlayView : FakeView, IGamePlayView
    {
        public int Updates;
        public string Moves = "";
        public bool CanUndo, CanHint;
        public string HintPrice = "";
        public void UpdateViewContent(Sprite image, string title, string about) { }
        public void UpdateControls(string moves, string hintPrice, bool canUndo, bool canHint) { Updates++; Moves = moves; HintPrice = hintPrice; CanUndo = canUndo; CanHint = canHint; }
    }

    internal sealed class FakeGameResultView : FakeView, IGameResultView
    {
        public string Progress;
        public bool DoubleOffered;
        public bool DoubleReady;
        public void UpdateViewContent(string stars, string stats, string themeName, ThemeProgress progress) => Progress = progress.ToString();

        public void ShowDoubleReward(bool offered, bool ready)
        {
            DoubleOffered = offered;
            DoubleReady = ready;
        }
    }

    internal sealed class FakeHeaderPanelView : IHeaderPanelView
    {
        public string Energy;
        public HeaderBtnType Button;
        public Action OnButton;
        public void UpdateViewContent(string starsAmount, string energyAmount) => Energy = energyAmount;

        public void UpdateCurrency(in CurrencyChangedEvent evt)
        {
            if (evt.Currency == Currency.Energy)
                Energy = evt.Balance.ToString();
        }

        public void ShowButton(HeaderBtnType type, Action onClick)
        {
            Button = type;
            OnButton = onClick;
        }
    }
}
