using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;
using Scripts.Systems;
using Scripts.UI;
using Scripts.UI.Presenters;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fives.Runtime.Tests
{
    public sealed class RepeatedStartTests
    {
        private TestObjects _objects;
        private EcsWorld _world;
        private GameSession _session;
        private EnergyService _energy;
        private GameStartService _start;
        private ThemeConfig _cities;
        private ThemeConfig _dogs;
        private PlayerProgressService _progress;
        private FakeSpriteLoader _loader;
        private FakeSelectMenuView _selectView;
        private SelectMenuPresenter _select;
        private FakeMainMenuView _mainView;
        private MainMenuPresenter _main;
        private FakeThemeDownloads _downloads;
        private FakeDownloadScreen _downloadScreen;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _world = new EcsWorld();
            var config = _objects.Config();
            config.DefaultUnlockedThemes = new[] { "cities" };
            _cities = config.Themes[0];
            _cities.Puzzles = _objects.Puzzles("cities", "One");
            _dogs = config.Themes[1];
            _dogs.Puzzles = _objects.Puzzles("dogs", "Rex");
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            _progress = new PlayerProgressService(save);
            _session = new GameSession(new GameBalance(config));
            _energy = new EnergyService(new GameBalance(config), save, new FakeClock { UtcNow = DateTime.UtcNow });
            _loader = new FakeSpriteLoader(_objects);
            _start = new GameStartService(_session, _energy, _world, _loader);
            _downloads = new FakeThemeDownloads { Everything = true };
            _downloadScreen = new FakeDownloadScreen();
            var gate = new ThemeDownloadGate(_downloads, _downloadScreen);
            _selectView = new FakeSelectMenuView();
            _select = new SelectMenuPresenter(config, _start, new ThemeShop(new StarService(save), _progress, _world, new GameBalance(config)), _progress, _session, new FakeHeaderPanelView(), _world, new FakeTexts(), _loader, _loader.Previews(config), gate);
            _select.Initialize(_selectView);
            _mainView = new FakeMainMenuView();
            _main = new MainMenuPresenter(config, _progress, _start, _session, new FakeHeaderPanelView(), _world, new FakeTexts(), _loader.Previews(config), gate);
            _main.Initialize(_mainView);
        }

        [TearDown]
        public void TearDown()
        {
            _world.Destroy();
            _objects.Dispose();
        }

        private int Requests => _world.Count<StartRunRequest>();

        // The run ends the way the game ends it: GameEndEvent through BoardDestroySystem.
        private void EndRun()
        {
            var systems = new EcsSystems(_world).Add(new BoardDestroySystem()).OneFrame<GameEndEvent>();
            systems.Init();
            _world.Send<GameEndEvent>();
            systems.Run();
            systems.Destroy();
        }

        private bool StartDirectly()
        {
            if (!_start.Prepare(_cities, _cities.Puzzles[0], CancellationToken.None).GetAwaiter().GetResult())
                return false;
            _start.Begin();
            return true;
        }

        private void StartFromBothMenus()
        {
            _selectView.OnClick("cities");       // Theme card → puzzle list.
            _selectView.OnClick("cities.one");   // Puzzle card, tapped twice.
            _selectView.OnClick("cities.one");
            _main.OnStartGame();
        }

        [Test]
        public void Start_IsAcceptedOnceAcrossBothMenus()
        {
            StartFromBothMenus();
            _selectView.Hide.TrySetResult();
            _mainView.Hide.TrySetResult();

            Assert.That(_energy.GetBalance(), Is.EqualTo(4));
            Assert.That(Requests, Is.EqualTo(1), "one board is requested");
        }

        [Test]
        public void Start_WaitsForTheHideAnimation_ThenPaysAndNavigatesOnce()
        {
            StartFromBothMenus();
            Assert.That(_world.Count<ChangeStateEvent>(), Is.Zero, "no navigation before the animation completes");
            Assert.That(_energy.GetBalance(), Is.EqualTo(5), "nothing is paid before the animation completes");

            _selectView.Hide.TrySetResult();
            Assert.That(_world.Count<ChangeStateEvent>(), Is.EqualTo(1));
            Assert.That(_energy.GetBalance(), Is.EqualTo(4));
        }

        [Test]
        public void NextRun_CanStart_ButNotWithoutEnergy()
        {
            StartFromBothMenus();
            _selectView.Hide.TrySetResult();
            EndRun();
            Assert.That(StartDirectly(), Is.True);
            Assert.That(_energy.GetBalance(), Is.EqualTo(3));

            EndRun();
            _energy.Spend(_energy.GetBalance());
            Assert.That(StartDirectly(), Is.False);
            Assert.That(Requests, Is.Zero);
        }

        [Test]
        public void Begin_WithoutPrepare_IsAProgrammingError()
        {
            Assert.Throws<InvalidOperationException>(() => _start.Begin());
        }

        [Test]
        public void MenuClosedWhileThePictureLoads_CostsNothing_AndTheNextStartWorks()
        {
            _loader.Slow = true;
            _main.OnStartGame();
            _mainView.Destroy(_main);
            _loader.FinishLoads();

            Assert.That(_energy.GetBalance(), Is.EqualTo(5));
            Assert.That(Requests, Is.Zero);
            Assert.That(_world.Count<ChangeStateEvent>(), Is.Zero);

            _loader.Slow = false;
            Assert.That(StartDirectly(), Is.True, "the cancelled start holds nothing back");
        }

        [Test]
        public void MenuClosedDuringItsHideAnimation_CostsNothing_AndTheNextStartWorks()
        {
            _selectView.OnClick("cities");
            _selectView.OnClick("cities.one");
            _selectView.Destroy(_select);

            Assert.That(_energy.GetBalance(), Is.EqualTo(5));
            Assert.That(Requests, Is.Zero);
            Assert.That(StartDirectly(), Is.True, "a preparation from a closed screen holds nothing back");
        }

        [Test]
        public void FailedLoad_CostsNothing_AndTheNextStartWorks()
        {
            _loader.Failing = true;
            LogAssert.Expect(LogType.Exception, new Regex("The bundle is missing"));
            _main.OnStartGame();

            Assert.That(_energy.GetBalance(), Is.EqualTo(5));
            Assert.That(Requests, Is.Zero);

            _loader.Failing = false;
            Assert.That(StartDirectly(), Is.True);
        }

        [Test]
        public void BackWhileThePuzzlesLoad_KeepsTheThemeList()
        {
            _loader.Slow = true;
            _selectView.OnClick("cities");
            _select.OnExit(); // the header's Back
            _loader.FinishLoads();

            Assert.That(_selectView.Items.Select(item => item.Id), Does.Contain("cities").And.Not.Contain("cities.one"));
        }

        [Test]
        public void TheLatestThemeTap_Wins()
        {
            _progress.Unlock(_dogs);
            _loader.Slow = true;
            var themes = _selectView.OnClick;
            themes("cities");
            themes("dogs");
            _loader.FinishLoads();

            Assert.That(_selectView.Items.Select(item => item.Id), Is.EqualTo(new[] { "dogs.rex" }));
        }

        [Test]
        public void ClosedScreen_ReleasesThePicturesItLoaded()
        {
            _loader.Slow = true;
            _selectView.OnClick("cities");
            _selectView.Destroy(_select);
            _loader.FinishLoads();

            Assert.That(_loader.Held.Keys.All(owner => owner is ThemePreviews), Is.True, "only the menu previews stay loaded");
        }

        [Test]
        public void AThemeNotOnTheDevice_DownloadsBeforeItsPuzzlesShow()
        {
            _downloads.Everything = false;
            _selectView.OnClick("cities");

            Assert.That(_downloads.Downloads, Is.EqualTo(1));
            Assert.That(_downloadScreen.Shown, Is.False, "the loading screen hides after the download");
            Assert.That(_selectView.Items.Select(item => item.Id), Is.EqualTo(new[] { "cities.one" }));
        }

        [Test]
        public void GivingUpADownload_KeepsTheThemeList_AndStartsNothing()
        {
            _downloads.Everything = false;
            _downloads.Failures = 2;
            _downloadScreen.Answers.Enqueue(false); // Back, on the theme screen
            _downloadScreen.Answers.Enqueue(false); // Back, on the main menu
            _selectView.OnClick("cities");
            _main.OnStartGame();

            Assert.That(_downloadScreen.Asked, Is.EqualTo(2));

            Assert.That(_selectView.Items.Select(item => item.Id), Does.Contain("cities").And.Not.Contain("cities.one"));
            Assert.That(_energy.GetBalance(), Is.EqualTo(5));
            Assert.That(Requests, Is.Zero);
        }
    }

    public sealed class ThemeCarouselTests
    {
        private TestObjects _objects;
        private EcsWorld _world;
        private GameSession _session;
        private GlobalConfig _config;
        private FakeMainMenuView _view;
        private FakeHeaderPanelView _header;
        private FakeSpriteLoader _sprites;
        private MainMenuPresenter _menu;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _world = new EcsWorld();
            _config = _objects.Config();
            _config.Themes[0].Puzzles = _objects.Puzzles("cities", "Paris");
            _config.Themes[1].Puzzles = _objects.Puzzles("dogs", "Corgi", "Pug", "Shepherd");
            _config.Themes.Add(_objects.Theme("cats", "Cats", 60, _objects.Puzzles("cats", "Siamese", "Ginger")));
            _config.DefaultUnlockedThemes = new[] { "dogs", "cats" };
            var save = new PlayerDataSaveHelper(new MemoryStorage(), _config, new GameBalance(_config));
            save.GetPlayerData().PlayerProgress.CompletedPuzzles.Add("dogs.corgi");
            _session = new GameSession(new GameBalance(_config));
            var energy = new EnergyService(new GameBalance(_config), save, new FakeClock { UtcNow = DateTime.UtcNow });
            _view = new FakeMainMenuView();
            _header = new FakeHeaderPanelView();
            _sprites = new FakeSpriteLoader(_objects);
            _menu = new MainMenuPresenter(_config, new PlayerProgressService(save), new GameStartService(_session, energy, _world, _sprites), _session, _header, _world, new FakeTexts(), _sprites.Previews(_config), FakeThemeDownloads.Ready());
            _menu.Initialize(_view);
            _view.Hide.TrySetResult();
        }

        [TearDown]
        public void TearDown()
        {
            _world.Destroy();
            _objects.Dispose();
        }

        [Test]
        public void HeaderButton_IsSettings_AndOpensTheSettingsScreen()
        {
            Assert.That(_header.Button, Is.EqualTo(HeaderBtnType.Settings));

            _header.OnButton();
            Assert.That(_world.Count<ChangeStateEvent>(), Is.EqualTo(1));
        }

        [Test]
        public void Opens_OnTheLastUnlockedTheme_WithItsPreview()
        {
            Assert.That(_view.Current.Title, Is.EqualTo("theme.cats"));
            Assert.That(_view.Current.Image, Is.EqualTo(_sprites.Of(_config.Themes[2].Preview)));
            Assert.That(_view.CanBrowse, Is.True);
            Assert.That(_view.Direction, Is.Zero);
        }

        [Test]
        public void ListsEveryTheme_LockedOnesIncluded_UntouchedAtZero()
        {
            Assert.That(_view.Previous.Title, Is.EqualTo("theme.dogs"));
            Assert.That(_view.Next.Title, Is.EqualTo("theme.cities"));
            Assert.That(_view.Next.Progress, Is.EqualTo("0/1"));
            Assert.That(_view.Next.Image, Is.EqualTo(_sprites.Of(_config.Themes[0].Preview)));
        }

        [Test]
        public void PlayOnALockedTheme_OpensTheThemeScreenFocusedOnIt()
        {
            _menu.OnNextTheme();
            _menu.OnStartGame();

            Assert.That(_world.Count<StartRunRequest>(), Is.Zero, "a locked theme does not start");
            Assert.That(_session.SelectedTheme, Is.EqualTo(_config.Themes[0]));
            Assert.That(_world.Count<ChangeStateEvent>(), Is.EqualTo(1));
        }

        [Test]
        public void Browsing_WrapsAround_AndPlayStartsTheCenteredThemesNextPuzzle()
        {
            _menu.OnNextTheme();
            _menu.OnNextTheme();

            var dogs = _config.Themes[1];
            Assert.That(_view.Current.Title, Is.EqualTo("theme.dogs"));
            Assert.That(_view.Current.Image, Is.EqualTo(_sprites.Of(dogs.Preview)));
            Assert.That(_view.Current.Progress, Is.EqualTo("1/3"));
            Assert.That(_view.Direction, Is.EqualTo(1));

            _menu.OnStartGame();
            Assert.That(_session.SelectedTheme, Is.EqualTo(dogs));
            Assert.That(_session.SelectedPuzzle, Is.EqualTo(dogs.Puzzles[1]));
        }
    }
}
