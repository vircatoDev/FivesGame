using System;
using Fives.Boot;
using Fives.Configs;
using Fives.Models;
using Fives.Services;
using Fives.UI;
using Fives.UI.Presenters;
using Leopotam.Ecs;

namespace Fives.Runtime.Tests
{
    /// <summary>
    /// The game's services wired as the installers wire them, over an in-memory save, a test clock and fake views.
    /// Each service is created on first use, so a test can change <see cref="Config"/> or the saved data before that.
    /// </summary>
    internal sealed class GameStand : IDisposable
    {
        public readonly TestObjects Objects = new TestObjects();
        public readonly EcsWorld World = new EcsWorld();
        public readonly GlobalConfig Config;
        public readonly IStorageService Storage;
        public readonly FakeClock Clock = new FakeClock { UtcNow = DateTime.UtcNow };
        public readonly FakeSpriteLoader Sprites;
        public readonly FakeHeaderPanelView Header = new FakeHeaderPanelView();
        public readonly FakeTexts Texts = new FakeTexts();
        public readonly FakeRewardedAds Ads = new FakeRewardedAds();

        private GameBalance _balance;
        private PlayerSave _save;
        private StarService _stars;
        private EnergyService _energy;
        private PlayerProgressService _progress;
        private GameSession _session;
        private ThemePreviews _previews;
        private GameStartService _start;
        private ThemeShop _shop;

        /// <param name="stars">The stars of a new save.</param>
        /// <param name="storage">Where the save is read and written; in memory by default.</param>
        public GameStand(int stars = TestConfig.Stars, IStorageService storage = null)
        {
            Config = Objects.Config(stars);
            Storage = storage ?? new MemoryStorage();
            Sprites = new FakeSpriteLoader(Objects);
        }

        public GameBalance Balance => _balance ??= new GameBalance(Config);
        public PlayerSave Save => _save ??= LoadSave();
        public StarService Stars => _stars ??= new StarService(Save);
        public EnergyService Energy => _energy ??= new EnergyService(Balance, Save, Clock);
        public PlayerProgressService Progress => _progress ??= new PlayerProgressService(Save);
        public GameSession Session => _session ??= new GameSession(Balance);
        public ThemePreviews Previews => _previews ??= Sprites.Previews(Config);
        public GameStartService Start => _start ??= new GameStartService(Session, Energy, World, Sprites);
        public ThemeShop Shop => _shop ??= new ThemeShop(Stars, Progress, World, Balance);

        public ThemeConfig Theme(string id) => Config.Themes.Find(theme => theme.Id == id);

        // Loaded at once, as the boot loads it before anything reads it.
        private PlayerSave LoadSave()
        {
            var save = new PlayerSave(Storage, Config, Balance);
            save.Load();
            return save;
        }

        public MainMenuPresenter MainMenu(FakeMainMenuView view)
        {
            var menu = new MainMenuPresenter(Config, Progress, Start, Session, Header, World, Previews);
            menu.Initialize(view);
            return menu;
        }

        public SelectMenuPresenter SelectMenu(FakeSelectMenuView view)
        {
            var menu = new SelectMenuPresenter(Config, Start, Shop, Progress, Session, Header, World, Texts, Sprites, Previews);
            menu.Initialize(view);
            return menu;
        }

        /// <summary>The boot of the app scope over this stand's services, with the given downloads and loading screen.</summary>
        public BootFlow Boot(IThemeDownloads downloads, ILoadingScreen screen) =>
            new BootFlow(new RemoteBalance(Balance), Save, new LocalizedTexts(new LanguageService(Config, Save)), Previews,
                downloads, Config, screen);

        /// <summary>The result screen's presenter, before the screen is shown.</summary>
        public GameResultPresenter Result() => new GameResultPresenter(Session, Stars, Progress, World, Texts, Ads);

        /// <summary>The gameplay screen's presenter, before the screen is shown.</summary>
        public GamePlayPresenter GamePlay() => new GamePlayPresenter(Session, Header, World, Texts);

        /// <summary>Picks a puzzle and begins a run on it, as the start of a run does before the board appears.</summary>
        public void BeginRun(ThemeConfig theme, PuzzleData puzzle)
        {
            Session.SetSelectedTheme(theme);
            Session.SetSelectedImage(puzzle, null);
            Session.BeginRun();
        }

        public void Dispose()
        {
            World.Destroy();
            Objects.Dispose();
        }
    }
}
