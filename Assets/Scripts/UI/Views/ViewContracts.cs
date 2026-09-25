using System;
using Cysharp.Threading.Tasks;
using Fives.Domain;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.UI.Views
{
    // Screen contracts that presenters and systems depend on. MonoBehaviour views implement them;
    // tests substitute plain fakes, so presentation logic runs without scenes or prefabs.

    public interface IView
    {
        UniTask PlayShowAnimation();
        UniTask PlayHideAnimation();
    }

    public interface IMainMenuView : IView
    {
        bool IsSliding { get; }
        void ShowThemes(in ThemeCard previous, in ThemeCard current, in ThemeCard next, int direction, bool canBrowse);
    }

    public interface ISelectMenuView : IView
    {
        void UpdateViewContent(MenuItemData[] newContent, string titleText, Action<string> onClick, bool playAnimation, int centeredItem = 0);
        void UnlockThemeItemByName(MenuItemData itemData, Action<string> onTileClick);
    }

    public interface IGamePlayView : IView
    {
        void UpdateViewContent(Sprite image, string title, string about);
        void UpdateControls(string moves, string hintPrice, bool canUndo, bool canHint);
    }

    public interface IGameResultView : IView
    {
        void UpdateViewContent(string stars, string stats, string themeName, ThemeProgress progress);
    }

    public interface IHeaderPanelView
    {
        void UpdateViewContent(string starsAmount, string energyAmount);
        void UpdateCurrency(in CurrencyChangedEvent evt);
        /// <summary>Shows the screen's header button; the callback stays in the presentation layer.</summary>
        void ShowButton(HeaderBtnType type, Action onClick);
    }
}
