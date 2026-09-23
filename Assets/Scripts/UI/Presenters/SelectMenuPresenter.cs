using System;
using System.Collections.Generic;
using System.Linq;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class SelectMenuPresenter : Presenter<SelectMenuView>
    {
        private readonly GameStartService _gameStartService;
        private readonly StarService _starService;
        private readonly PlayerProgressService _playerProgressService;
        private readonly List<ThemeConfig> _themeConfig;
        private readonly EcsWorld _world;

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

            await View.PlayHideAnimation();
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
            View.UpdateViewContent(tiles, "SELECT THEME", OnThemeSelected, playAnimation);
            ClearSelectedTheme();
        }

        private MenuItemData[] GetThemeItemData()
        {
            return _themeConfig.Select(ToMenuItem).ToArray();
        }

        private MenuItemData ToMenuItem(ThemeConfig theme)
        {
            var unlocked = IsThemeUnlocked(theme);
            return new MenuItemData
            {
                Id = theme.ThemeName,
                Image = theme.ThemeLogo,
                TitleText = theme.ThemeName,
                BottomText = unlocked ? GetProgressText(theme) : $"Open {theme.UnlockCost}",
                Offer = !unlocked
            };
        }

        private string GetProgressText(ThemeConfig theme)
        {
            var progress = _playerProgressService.GetThemeProgress(theme);
            return progress.IsComplete ? "COMPLETED" : progress.ToString();
        }

        private bool IsThemeUnlocked(ThemeConfig theme)
        {
            return _playerProgressService.GetProgressData().UnlockedThemes.Contains(theme.ThemeName);
        }

        private bool IsPuzzleUnlocked(string puzzleName)
        {
            return _playerProgressService.GetProgressData().CompletedPuzzles.Contains(puzzleName);
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

            View.UpdateViewContent(tiles, "SELECT PUZZLE", OnStartGame, true);
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
            View.UnlockThemeItemByName(ToMenuItem(theme), OnThemeSelected);
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

        private void ShowNoStarsAnimation() => _world.Send(CurrencyChangedEvent.NotEnough(Currency.Stars));

        private void UpdateStarBalance(int amount) =>
            _world.Send(CurrencyChangedEvent.Changed(Currency.Stars, _starService.GetBalance(), amount));

        private void SaveStarData() => _world.Send(new SaveDataEvent { StorableObject = _starService });
        private void SaveProgressData() => _world.Send(new SaveDataEvent { StorableObject = _playerProgressService });
    }
}
