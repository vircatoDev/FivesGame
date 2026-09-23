using System.Linq;
using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class MainMenuPresenter : Presenter<MainMenuView>
    {
        private readonly GlobalConfig _themeConfig;
        private readonly PlayerProgressService _playerProgressService;
        private readonly GameStartService _gameStartService;
        private readonly EcsWorld _world;

        public MainMenuPresenter(GlobalConfig themeConfig, PlayerProgressService playerProgressService,
            GameStartService gameStartService, EcsWorld world)
        {
            _themeConfig = themeConfig;
            _playerProgressService = playerProgressService;
            _gameStartService = gameStartService;
            _world = world;
        }

        public override void OnActivateView()
        {
            var theme = GetLastActiveTheme();

            View.PlayShowAnimation().Forget();
            View.UpdateViewContent(_playerProgressService.GetThemeProgress(theme).ToString(), theme.ThemeName, theme.ThemeLogo);

            _world.Send(new UpdateControlPanelBtnLogicEvent { CommonBtnCallback = OnSettings, BtnType = HeaderBtnType.Settings });
        }

        public async void OnStartGame()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);

            var theme = GetLastActiveTheme();
            var nextPuzzle = FindNextUncompletedPuzzle(theme);
            if (!_gameStartService.TryStart(theme, nextPuzzle))
                return;
            
            await View.PlayHideAnimation();
            
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
            await View.PlayHideAnimation();
            _world.ChangeState(GameStateType.SelectMenu);
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
    }
}
