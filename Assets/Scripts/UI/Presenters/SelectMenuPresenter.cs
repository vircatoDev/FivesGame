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
        private readonly GameSession _session;
        private readonly EcsWorld _world;

        private string _selectedTheme;

        public SelectMenuPresenter(
            GlobalConfig gameConfig,
            GameStartService gameStartService,
            StarService starService,
            PlayerProgressService playerProgressService,
            GameSession session,
            EcsWorld world)
        {
            _themeConfig = gameConfig.Themes;
            _gameStartService = gameStartService;
            _starService = starService;
            _playerProgressService = playerProgressService;
            _session = session;
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

        private PuzzleData GetPuzzleData(string puzzleId) =>
            GetSelectedTheme()?.Puzzles.FirstOrDefault(p => p.Id == puzzleId);

        private ThemeConfig GetSelectedTheme() => GetTheme(_selectedTheme);

        private void ChangeGameState(GameStateType newState) => _world.ChangeState(newState);

        private void UpdateThemeSelectionView(bool playAnimation)
        {
            // Center the theme just left, or the one the main menu asked for.
            var focus = GetSelectedTheme() ?? _session.SelectedTheme;
            var tiles = GetThemeItemData();
            View.UpdateViewContent(tiles, "SELECT THEME", OnThemeSelected, playAnimation, Math.Max(0, _themeConfig.IndexOf(focus)));
            ClearSelectedTheme();
        }

        private MenuItemData[] GetThemeItemData()
        {
            return _themeConfig.Select(ToMenuItem).ToArray();
        }

        private MenuItemData ToMenuItem(ThemeConfig theme)
        {
            var unlocked = _playerProgressService.IsUnlocked(theme);
            return new MenuItemData
            {
                Id = theme.Id,
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

        private void OnThemeSelected(string themeId)
        {
            PlaySoundEffect(AudioKeyCollection.MenuClick);

            _selectedTheme = themeId;

            var puzzles = GetTheme(themeId)?.Puzzles ?? Array.Empty<PuzzleData>();
            var tiles = puzzles.Select(puzzle => new MenuItemData
            {
                Id = puzzle.Id,
                Image = puzzle.Image,
                TitleText = puzzle.Name,
                BottomText = _playerProgressService.IsCompleted(puzzle) ? "Complete" : "Play"
            }).ToArray();

            View.UpdateViewContent(tiles, "SELECT PUZZLE", OnStartGame, true);
        }

        public void OnThemeBuy(string themeId)
        {
            PlaySoundEffect(AudioKeyCollection.MenuClick);

            var theme = GetTheme(themeId);
            if (theme == null || _playerProgressService.IsUnlocked(theme))
                return;

            if (theme.UnlockCost < 0 || (theme.UnlockCost > 0 && !_starService.Spend(theme.UnlockCost)))
            {
                PlaySoundEffect(AudioKeyCollection.WrongClick);
                ShowNoStarsAnimation();
                return;
            }

            UnlockTheme(theme);
        }

        private ThemeConfig GetTheme(string themeId) => _themeConfig.FirstOrDefault(t => t.Id == themeId);

        private void UnlockTheme(ThemeConfig theme)
        {
            UpdateStarBalance(-theme.UnlockCost);
            SaveStarData();

            _playerProgressService.Unlock(theme);
            SaveProgressData();
            View.UnlockThemeItemByName(ToMenuItem(theme), OnThemeSelected);
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
