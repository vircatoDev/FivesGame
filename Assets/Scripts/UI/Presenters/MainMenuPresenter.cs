using System;
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
        private readonly GameSession _session;
        private readonly EcsWorld _world;
        private ThemeConfig[] _themes;
        private int _themeIndex;

        public MainMenuPresenter(GlobalConfig themeConfig, PlayerProgressService playerProgressService,
            GameStartService gameStartService, GameSession session, EcsWorld world)
        {
            _themeConfig = themeConfig;
            _playerProgressService = playerProgressService;
            _gameStartService = gameStartService;
            _session = session;
            _world = world;
        }

        public override void OnActivateView()
        {
            // Every theme with puzzles, locked ones included: Play on a locked theme leads to its purchase.
            _themes = _themeConfig.Themes.Where(theme => theme.Puzzles.Length > 0).ToArray();
            _themeIndex = Array.IndexOf(_themes, GetLastActiveTheme());

            View.PlayShowAnimation().Forget();
            ShowThemes(0);

            _world.Send(new UpdateControlPanelBtnLogicEvent { CommonBtnCallback = OnSettings, BtnType = HeaderBtnType.Settings });
        }

        public async void OnStartGame()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);

            var theme = _themes[_themeIndex];
            if (!_playerProgressService.GetProgressData().UnlockedThemes.Contains(theme.ThemeName))
            {
                // A locked theme is bought on the theme screen, which opens centered on it.
                await OpenThemeScreen(theme);
                return;
            }

            if (!_gameStartService.TryStart(theme, FindNextUncompletedPuzzle(theme)))
                return;
            
            await View.PlayHideAnimation();
            
            _world.ChangeState(GameStateType.Playing);
        }

        public void OnPreviousTheme() => BrowseThemes(-1);

        public void OnNextTheme() => BrowseThemes(1);

        private void BrowseThemes(int step)
        {
            if (View.IsSliding)
                return;

            _world.PlaySound(AudioKeyCollection.MenuClick);
            _themeIndex = Wrap(_themeIndex + step);
            ShowThemes(step);
        }

        private void ShowThemes(int direction) =>
            View.ShowThemes(Card(_themes[Wrap(_themeIndex - 1)]), Card(_themes[_themeIndex]),
                Card(_themes[Wrap(_themeIndex + 1)]), direction, _themes.Length > 1);

        // A theme card shows the puzzle Play would start: the first uncompleted one, or the last when all are done.
        private ThemeCard Card(ThemeConfig theme) => new ThemeCard(FindNextUncompletedPuzzle(theme).Image,
            theme.ThemeName, _playerProgressService.GetThemeProgress(theme).ToString());

        private int Wrap(int index) => (index % _themes.Length + _themes.Length) % _themes.Length;

        public void OnSettings()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);
            _world.ChangeState(GameStateType.Settings);
        }

        public async void OnSelectMenu()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);
            await OpenThemeScreen(_themes[_themeIndex]);
        }

        private async UniTask OpenThemeScreen(ThemeConfig focus)
        {
            _session.SetSelectedTheme(focus);
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
