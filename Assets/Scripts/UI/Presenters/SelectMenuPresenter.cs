using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Scripts.Commands;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.UI.Presenters
{
    public class SelectMenuPresenter : BasePresenter
    {
        private readonly GameSession _gameSession;
        private readonly EnergyService _energyService;
        private readonly StarService _starService;
        private readonly PlayerProgressService _playerProgressService;
        private readonly List<ThemeConfig> _themeConfig;
        private readonly ECSCommandService _ecsCommandService;

        private SelectMenuView _view;
        private string _selectedTheme;

        public SelectMenuPresenter(
            GlobalConfig gameConfig,
            GameSession gameSession,
            EnergyService energyService,
            StarService starService,
            PlayerProgressService playerProgressService,
            ECSCommandService ecsCommandService)
        {
            _themeConfig = gameConfig.Themes;
            _gameSession = gameSession;
            _energyService = energyService;
            _starService = starService;
            _playerProgressService = playerProgressService;
            _ecsCommandService = ecsCommandService;
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

            if (!TrySpendEnergy())
                return;

            var imageData = GetPuzzleData(selectedImg);
            if (imageData == null)
            {
                return;
            }

            SetSelectedGameSettings(imageData);
            await HideCurrentView();
            ChangeGameState(GameStateType.Playing);
        }

        public void OnSettings()
        {
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

        private bool TrySpendEnergy()
        {
            if (_energyService.GetBalance() <= 0)
            {
                PlaySoundEffect(AudioKeyCollection.WrongClick);
                ShowNoEnergyAnimation();
                return false;
            }

            _energyService.Spend(1);
            UpdateEnergyBalance();
            SaveEnergyData();
            return true;
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

        private void SetSelectedGameSettings(PuzzleData imageData)
        {
            _gameSession.SetSelectedImage(imageData);
            _gameSession.SetSelectedTheme(GetSelectedTheme());
        }

        private async UniTask HideCurrentView()
        {
            await _view.PlayHideAnimation();
        }

        private void ChangeGameState(GameStateType newState)
        {
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(newState).Execute();
        }

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

            if (!_starService.Spend(theme.UnlockCost))
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
            _starService.Spend(theme.UnlockCost);
            UpdateStarBalance(-theme.UnlockCost);
            SaveStarData();

            _playerProgressService.UnlockTheme(theme.ThemeName);
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

        private void PlaySoundEffect(string key) =>
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(key, 1f).Execute();

        private void UpdateHeaderButton() => _ecsCommandService
            .CreateCommand<UpdateHeaderBtnLogicCommand>(OnExit, HeaderBtnType.Back).Execute();

        private void ShowNoEnergyAnimation() =>
            _ecsCommandService.CreateCommand<HeaderNoEnergyAnimationCommand>().Execute();

        private void ShowNoStarsAnimation() => _ecsCommandService.CreateCommand<HeaderNoStarsAnimationCommand>().Execute();

        private void UpdateEnergyBalance() =>
            _ecsCommandService.CreateCommand<UpdateEnergyBalanceCommand>(_energyService, -1).Execute();

        private void SaveEnergyData() => _ecsCommandService.CreateCommand<SaveDataCommand>(_energyService).Execute();

        private void UpdateStarBalance(int amount) =>
            _ecsCommandService.CreateCommand<UpdateStarBalanceCommand>(_starService, amount).Execute();

        private void SaveStarData() => _ecsCommandService.CreateCommand<SaveDataCommand>(_starService).Execute();
    }
}