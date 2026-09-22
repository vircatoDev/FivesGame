using System.Collections.Generic;
using System.Linq;
using Scripts.Commands;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.UI.Presenters
{
    public class MainMenuPresenter : BasePresenter
    {
        private readonly GlobalConfig _themeConfig;
        private readonly PlayerProgressService _playerProgressService;
        private readonly GameStartService _gameStartService;
        private readonly ECSCommandService _ecsCommandService;
        private MainMenuView _view;

        public MainMenuPresenter(GlobalConfig themeConfig, PlayerProgressService playerProgressService,
            GameStartService gameStartService, ECSCommandService ecsCommandService)
        {
            _themeConfig = themeConfig;
            _playerProgressService = playerProgressService;
            _gameStartService = gameStartService;
            _ecsCommandService = ecsCommandService;
        }

        public override void Initialize(BaseView initData)
        {
            _view = initData as MainMenuView;
            OnActivateView();
        }

        public override void OnActivateView()
        {
            var theme = GetLastActiveTheme();

            _view.PlayShowAnimation();
            _view.UpdateViewContent(GetThemeProgress(theme), theme.ThemeName, theme.ThemeLogo);

            _ecsCommandService.CreateCommand<UpdateHeaderBtnLogicCommand>(OnSettings, HeaderBtnType.Settings).Execute();
        }

        public async void OnStartGame()
        {
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.MenuClick, 1f).Execute();

            var theme = GetLastActiveTheme();
            var nextPuzzle = FindNextUncompletedPuzzle(theme);
            if (!_gameStartService.TryStart(theme, nextPuzzle))
                return;
            
            await _view.PlayHideAnimation();
            
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.Playing).Execute();
        }

        public void OnSettings()
        {
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.MenuClick, 1f).Execute();
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.Settings).Execute();
        }

        public async void OnSelectMenu()
        {
            Debug.Log("MainMenuPresenter: Start Game clicked");
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.MenuClick, 1f).Execute();
            await _view.PlayHideAnimation();
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.SelectMenu).Execute();
        }

        private List<string> GetCompletedPuzzleForThemeIntersect(ThemeConfig theme)
        {
            var completedPuzzles = _playerProgressService.GetProgressData().CompletedPuzzles;
            return completedPuzzles.Intersect(theme.Puzzles.Select(p => p.Name)).ToList();
        }

        private ThemeConfig GetLastActiveTheme()
        {
            var lastActiveTheme = _playerProgressService.GetProgressData().UnlockedThemes
                .Last(name => _themeConfig.Themes.Any(theme => theme.ThemeName == name));
            var theme = _themeConfig.Themes.FirstOrDefault(x => x.ThemeName == lastActiveTheme);
            return theme;
        }

        private PuzzleData FindNextUncompletedPuzzle(ThemeConfig lastUnlockedTheme)
        {
            var completedPuzzles = _playerProgressService.GetProgressData().CompletedPuzzles;

            foreach (var puzzle in lastUnlockedTheme.Puzzles)
            {
                if (!completedPuzzles.Contains(puzzle.Name))
                {
                    return puzzle;
                }
            }

            return lastUnlockedTheme.Puzzles.Last();
        }

        private string GetThemeProgress(ThemeConfig theme) =>
            GetCompletedPuzzleForThemeIntersect(theme).Count() + "/" + theme.Puzzles.Length;
    }
}
