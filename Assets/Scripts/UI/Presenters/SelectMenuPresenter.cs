using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class SelectMenuPresenter : Presenter<ISelectMenuView>
    {
        private readonly GameStartService _gameStartService;
        private readonly ThemeShop _shop;
        private readonly PlayerProgressService _playerProgressService;
        private readonly List<ThemeConfig> _themeConfig;
        private readonly GameSession _session;
        private readonly IHeaderPanelView _header;
        private readonly EcsWorld _world;
        private readonly ITexts _texts;
        private readonly ISpriteLoader _sprites;
        private readonly ThemePreviews _previews;
        // Owner of the open theme's puzzle pictures: its bundle is released when the theme list comes back.
        private readonly object _themePictures = new object();
        // The puzzle list being loaded: a newer tap, the way back to the themes or closing the screen cancels it.
        private CancellationTokenSource _puzzlesLoad;

        private string _selectedTheme;

        public SelectMenuPresenter(
            GlobalConfig gameConfig,
            GameStartService gameStartService,
            ThemeShop shop,
            PlayerProgressService playerProgressService,
            GameSession session,
            IHeaderPanelView header,
            EcsWorld world,
            ITexts texts,
            ISpriteLoader sprites,
            ThemePreviews previews)
        {
            _themeConfig = gameConfig.Themes;
            _gameStartService = gameStartService;
            _shop = shop;
            _playerProgressService = playerProgressService;
            _session = session;
            _header = header;
            _world = world;
            _texts = texts;
            _sprites = sprites;
            _previews = previews;
        }

        public override void OnActivateView()
        {
            UpdateHeaderButton();
            ShowThemes(false);
        }

        protected override void OnDeactivateView()
        {
            CancelPuzzlesLoad();
            _sprites.Release(_themePictures);
        }

        public void OnStartGame(string puzzleId) => StartGameAsync(puzzleId).Forget();

        // A closed screen cancels the load or the animation, and the run is then never begun.
        private async UniTaskVoid StartGameAsync(string puzzleId)
        {
            PlaySoundEffect(AudioKeyCollection.MenuClick);

            if (!await _gameStartService.Prepare(GetSelectedTheme(), GetPuzzleData(puzzleId), View.Lifetime))
                return;

            await View.PlayHideAnimation();
            _gameStartService.Begin();
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
                ShowThemes(true);
            }
        }

        private PuzzleData GetPuzzleData(string puzzleId) =>
            GetSelectedTheme()?.Puzzles.FirstOrDefault(p => p.Id == puzzleId);

        private ThemeConfig GetSelectedTheme() => GetTheme(_selectedTheme);

        private void ChangeGameState(GameStateType newState) => _world.ChangeState(newState);

        private void CancelPuzzlesLoad()
        {
            _puzzlesLoad?.Cancel();
            _puzzlesLoad?.Dispose();
            _puzzlesLoad = null;
        }

        private void ShowThemes(bool playAnimation)
        {
            CancelPuzzlesLoad();
            _sprites.Release(_themePictures);

            // Center the theme just left, or the one the main menu asked for.
            var focus = GetSelectedTheme() ?? _session.SelectedTheme;
            var tiles = GetThemeItemData();
            View.UpdateViewContent(tiles, _texts.Get(TextKeys.SelectThemes), OnThemeSelected, playAnimation, Math.Max(0, _themeConfig.IndexOf(focus)));
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
                Image = _previews.Of(theme),
                TitleText = _texts.Get(TextKeys.Name(theme)),
                BottomText = unlocked ? GetProgressText(theme) : _texts.Get(TextKeys.Open, theme.UnlockCost),
                Offer = !unlocked
            };
        }

        private string GetProgressText(ThemeConfig theme)
        {
            var progress = _playerProgressService.GetThemeProgress(theme);
            return progress.IsComplete ? _texts.Get(TextKeys.ThemeCompleted) : progress.ToString();
        }

        private void OnThemeSelected(string themeId) => ShowPuzzles(themeId).Forget();

        // Loads the theme bundle: its full-size pictures fill the puzzle cards and start the game.
        // The latest tap wins: an earlier load that finishes later is cancelled and shows nothing. Pictures it did load
        // stay with _themePictures and are released with the rest.
        private async UniTaskVoid ShowPuzzles(string themeId)
        {
            PlaySoundEffect(AudioKeyCollection.MenuClick);

            _selectedTheme = themeId;
            CancelPuzzlesLoad();
            _puzzlesLoad = CancellationTokenSource.CreateLinkedTokenSource(View.Lifetime);

            var puzzles = GetTheme(themeId)?.Puzzles ?? Array.Empty<PuzzleData>();
            var pictures = await UniTask.WhenAll(puzzles.Select(puzzle => _sprites.Load(puzzle.Image, _themePictures)))
                .AttachExternalCancellation(_puzzlesLoad.Token);
            var tiles = puzzles.Select((puzzle, i) => new MenuItemData
            {
                Id = puzzle.Id,
                Image = pictures[i],
                TitleText = _texts.Get(TextKeys.Name(puzzle)),
                BottomText = _texts.Get(_playerProgressService.IsCompleted(puzzle) ? TextKeys.PuzzleCompleted : TextKeys.Play)
            }).ToArray();

            View.UpdateViewContent(tiles, _texts.Get(TextKeys.SelectPuzzles), OnStartGame, true);
        }

        public void OnThemeBuy(string themeId)
        {
            var theme = GetTheme(themeId);
            if (theme == null)
                return;

            switch (_shop.TryUnlock(theme))
            {
                case PurchaseResult.Unlocked:
                    PlaySoundEffect(AudioKeyCollection.MenuClick);
                    View.UnlockThemeItemByName(ToMenuItem(theme), OnThemeSelected);
                    break;
                case PurchaseResult.NotEnoughStars:
                    PlaySoundEffect(AudioKeyCollection.WrongClick);
                    break;
            }
        }

        private ThemeConfig GetTheme(string themeId) => _themeConfig.FirstOrDefault(t => t.Id == themeId);

        private void BackToMainMenu() => ChangeGameState(GameStateType.MainMenu);

        private void ClearSelectedTheme() => _selectedTheme = string.Empty;

        private void PlaySoundEffect(string key) => _world.PlaySound(key);

        private void UpdateHeaderButton() =>
            _header.ShowButton(HeaderBtnType.Back, OnExit);
    }
}
