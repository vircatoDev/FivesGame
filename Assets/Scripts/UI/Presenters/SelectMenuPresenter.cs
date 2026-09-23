using System;
using System.Collections.Generic;
using System.Linq;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.UI.Presenters
{
    public class SelectMenuPresenter : BasePresenter
    {
        private readonly GameStartService _gameStartService;
        private readonly StarService _starService;
        private readonly PlayerProgressService _playerProgressService;
        private readonly List<ThemeConfig> _themeConfig;
        private readonly EcsWorld _world;

        private SelectMenuView _view;
        private string _selectedTheme;

        public SelectMenuPresenter(
            GlobalConfig gameConfig,
            GameStartService gameStartService,
            StarService starService,
            PlayerProgressService playerProgressService,
            EcsWorld world)
        {
            _themeConfig = gameConfig.Themes;
            _gameStartService = gameStartService;
            _starService = starService;
            _playerProgressService = playerProgressService;
            _world = world;
        }

        public override void Initialize(BaseView initData)
        {
            if (initData is not SelectMenuView view)
            {
                Debug.LogError("SelectMenuPresenter: wrong initData.");
                return;
            }

            _view = view;
            OnActivateView();
        }

        public override void OnActivateView()
        {
            UpdateHeaderButton();
            UpdateThemeSelectionView(false);
        }

        public async void OnStartGame(string selectedImg)
        {
            PlaySoundEffect(AudioKeyCollection.MenuClick);

            var imageData = GetPuzzleData(selectedImg);
            if (!_gameStartService.TryStart(GetSelectedTheme(), imageData))
                return;

            await _view.PlayHideAnimation();
            ChangeGameState(GameStateType.Playing);
        }

        public void OnExit()
        {
            PlaySoundEffect(AudioKeyCollection.MenuClick);

            if (string.IsNullOrEmpty(_selectedTheme))
            {
                BackToMainMenu();
            }
            else
            {
                UpdateThemeSelectionView(true);
            }
        }

        private PuzzleData GetPuzzleData(string puzzleName)
        {
            var theme = GetSelectedTheme();
            return theme?.Puzzles.FirstOrDefault(p => p.Name == puzzleName);
        }

        private ThemeConfig GetSelectedTheme()
        {
            return _themeConfig.FirstOrDefault(t => t.ThemeName == _selectedTheme);
        }

        private void ChangeGameState(GameStateType newState) => _world.ChangeState(newState);

        private void UpdateThemeSelectionView(bool playAnimation)
        {
            var tiles = GetThemeItemData();
            _view.UpdateViewContent(tiles, "SELECT THEME", OnThemeSelected, playAnimation);
            ClearSelectedTheme();
        }

        private MenuItemData[] GetThemeItemData()
        {
            return _themeConfig.Select(theme => new MenuItemData
            {
                Id = theme.ThemeName,
                Image = theme.ThemeLogo,
                TitleText = theme.ThemeName,
                BottomText = CreateTextForThemeTile(theme),
                Offer = !IsThemeUnlocked(theme)
            }).ToArray();
        }

        private string CreateTextForThemeTile(ThemeConfig theme)
        {
            if (IsThemeUnlocked(theme))
            {
                var progress = GetThemeProgress(theme);
                return progress == theme.Puzzles.Length.ToString() ? "COMPLETED" : $"{progress}/{theme.Puzzles.Length}";
            }

            return $"Open {theme.UnlockCost}";
        }

        private bool IsThemeUnlocked(ThemeConfig theme)
        {
            return _playerProgressService.GetProgressData().UnlockedThemes.Contains(theme.ThemeName);
        }

        private bool IsPuzzleUnlocked(string puzzleName)
        {
            return _playerProgressService.GetProgressData().CompletedPuzzles.Contains(puzzleName);
        }

        private string GetThemeProgress(ThemeConfig theme)
        {
            var completedPuzzles = _playerProgressService.GetProgressData().CompletedPuzzles;
            return completedPuzzles.Intersect(theme.Puzzles.Select(p => p.Name)).Count().ToString();
        }

        private void OnThemeSelected(string themeName)
        {
            PlaySoundEffect(AudioKeyCollection.MenuClick);

            _selectedTheme = themeName;

            var tiles = GetAllImagesByThemeName(themeName).Select(data => new MenuItemData
            {
                Id = data.Name,
                Image = data.Image,
                TitleText = data.Name,
                BottomText = IsPuzzleUnlocked(data.Name) ? "COMPLETED" : string.Empty
            }).ToArray();

            _view.UpdateViewContent(tiles, "SELECT PUZZLE", OnStartGame, true);
        }

        public void OnThemeBuy(string themeName)
        {
            PlaySoundEffect(AudioKeyCollection.MenuClick);

            var theme = GetThemeByName(themeName);

            if (theme == null)
            {
                return;
            }

            if (IsThemeUnlocked(theme))
            {
                return;
            }

            if (theme.UnlockCost < 0 || (theme.UnlockCost > 0 && !_starService.Spend(theme.UnlockCost)))
            {
                PlaySoundEffect(AudioKeyCollection.WrongClick);
                ShowNoStarsAnimation();
                return;
            }

            UnlockTheme(theme);
        }

        private ThemeConfig GetThemeByName(string themeName)
        {
            return _themeConfig.FirstOrDefault(t => t.ThemeName == themeName);
        }

        private void UnlockTheme(ThemeConfig theme)
        {
            UpdateStarBalance(-theme.UnlockCost);
            SaveStarData();

            _playerProgressService.UnlockTheme(theme.ThemeName);
            SaveProgressData();
            var updatedTile = new MenuItemData
            {
                Id = theme.ThemeName,
                Image = theme.ThemeLogo,
                TitleText = theme.ThemeName,
                BottomText = CreateTextForThemeTile(theme),
                Offer = false
            };

            _view.UnlockThemeItemByName(updatedTile, OnThemeSelected);
        }

        private PuzzleData[] GetAllImagesByThemeName(string themeName)
        {
            var theme = GetThemeByName(themeName);
            if (theme == null)
            {
                return Array.Empty<PuzzleData>();
            }

            return theme.Puzzles;
        }

        private void BackToMainMenu() => ChangeGameState(GameStateType.MainMenu);

        private void ClearSelectedTheme() => _selectedTheme = string.Empty;

        private void PlaySoundEffect(string key) => _world.PlaySound(key);

        private void UpdateHeaderButton() =>
            _world.Send(new UpdateControlPanelBtnLogicEvent { CommonBtnCallback = OnExit, BtnType = HeaderBtnType.Back });

        private void ShowNoStarsAnimation() => _world.Send(new UpdateControlPanelStarsEvent { StarsNotEnough = true });

        private void UpdateStarBalance(int amount) =>
            _world.Send(new UpdateControlPanelStarsEvent { StarsAmount = _starService.GetBalance(), StarsChange = amount });

        private void SaveStarData() => _world.Send(new SaveDataEvent { StorableObject = _starService });
        private void SaveProgressData() => _world.Send(new SaveDataEvent { StorableObject = _playerProgressService });
    }
}
