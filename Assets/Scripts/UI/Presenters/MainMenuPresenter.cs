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
    public class MainMenuPresenter : Presenter<IMainMenuView>
    {
        private readonly GlobalConfig _themeConfig;
        private readonly PlayerProgressService _playerProgressService;
        private readonly GameStartService _gameStartService;
        private readonly GameSession _session;
        private readonly IHeaderPanelView _header;
        private readonly EcsWorld _world;
        private readonly ITexts _texts;
        private readonly ThemePreviews _previews;
        private readonly ThemeDownloadGate _downloads;
        private ThemeConfig[] _themes;
        private int _themeIndex;

        public MainMenuPresenter(GlobalConfig themeConfig, PlayerProgressService playerProgressService,
            GameStartService gameStartService, GameSession session, IHeaderPanelView header, EcsWorld world, ITexts texts, ThemePreviews previews,
            ThemeDownloadGate downloads)
        {
            _themeConfig = themeConfig;
            _playerProgressService = playerProgressService;
            _gameStartService = gameStartService;
            _session = session;
            _header = header;
            _world = world;
            _texts = texts;
            _previews = previews;
            _downloads = downloads;
        }

        public override void OnActivateView()
        {
            // Every theme with puzzles, locked ones included: Play on a locked theme leads to its purchase.
            _themes = _themeConfig.Themes.Where(theme => theme.Puzzles.Length > 0).ToArray();
            _themeIndex = Array.IndexOf(_themes, GetLastActiveTheme());
            _header.ShowButton(HeaderBtnType.Settings, OnSettings);
            View.PlayShowAnimation().Forget();
            ShowThemes(0);
        }

        public void OnStartGame() => StartGameAsync().Forget();

        // A closed screen cancels the load or the animation, and the run is then never begun.
        private async UniTaskVoid StartGameAsync()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);

            var theme = _themes[_themeIndex];
            if (!_playerProgressService.IsUnlocked(theme))
            {
                // A locked theme is bought on the theme screen, which opens centered on it.
                await OpenThemeScreen(theme);
                return;
            }

            if (!await _downloads.Ensure(theme, View.Lifetime))
                return;
            if (!await _gameStartService.Prepare(theme, FindNextUncompletedPuzzle(theme), View.Lifetime))
                return;

            await View.PlayHideAnimation();
            _gameStartService.Begin();
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

        private ThemeCard Card(ThemeConfig theme) => new ThemeCard(_previews.Of(theme),
            _texts.Get(TextKeys.Name(theme)), _playerProgressService.GetThemeProgress(theme).ToString());

        private int Wrap(int index) => (index % _themes.Length + _themes.Length) % _themes.Length;

        public void OnSettings()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);
            _world.ChangeState(GameStateType.Settings);
        }

        public void OnSelectMenu()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);
            OpenThemeScreen(_themes[_themeIndex]).Forget();
        }

        private async UniTask OpenThemeScreen(ThemeConfig focus)
        {
            _session.SetSelectedTheme(focus);
            await View.PlayHideAnimation();
            _world.ChangeState(GameStateType.SelectMenu);
        }

        // The most recently unlocked theme; the first theme when no unlocked id matches the config
        // (for example, a save from a build with themes that were later removed).
        private ThemeConfig GetLastActiveTheme()
        {
            var unlocked = _playerProgressService.GetProgressData().UnlockedThemes;
            for (var i = unlocked.Count - 1; i >= 0; i--)
            {
                var theme = _themes.FirstOrDefault(t => t.Id == unlocked[i]);
                if (theme != null)
                    return theme;
            }

            return _themes[0];
        }

        private PuzzleData FindNextUncompletedPuzzle(ThemeConfig theme) =>
            theme.Puzzles.FirstOrDefault(puzzle => !_playerProgressService.IsCompleted(puzzle)) ?? theme.Puzzles.Last();
    }
}
