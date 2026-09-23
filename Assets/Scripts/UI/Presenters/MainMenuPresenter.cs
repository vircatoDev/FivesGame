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
    public class MainMenuPresenter : BasePresenter
    {
        private readonly GlobalConfig _themeConfig;
        private readonly PlayerProgressService _playerProgressService;
        private readonly GameStartService _gameStartService;
        private readonly EcsWorld _world;
        private MainMenuView _view;

        public MainMenuPresenter(GlobalConfig themeConfig, PlayerProgressService playerProgressService,
            GameStartService gameStartService, EcsWorld world)
        {
            _themeConfig = themeConfig;
            _playerProgressService = playerProgressService;
            _gameStartService = gameStartService;
            _world = world;
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

            _world.Send(new UpdateControlPanelBtnLogicEvent { CommonBtnCallback = OnSettings, BtnType = HeaderBtnType.Settings });
        }

        public async void OnStartGame()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);

            var theme = GetLastActiveTheme();
            var nextPuzzle = FindNextUncompletedPuzzle(theme);
            if (!_gameStartService.TryStart(theme, nextPuzzle))
                return;
            
            await _view.PlayHideAnimation();
            
            _world.ChangeState(GameStateType.Playing);
        }

        public void OnSettings()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);
            _world.ChangeState(GameStateType.Settings);
        }

        public async void OnSelectMenu()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);
            await _view.PlayHideAnimation();
            _world.ChangeState(GameStateType.SelectMenu);
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
