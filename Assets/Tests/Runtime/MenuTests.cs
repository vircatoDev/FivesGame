using System;
using System.Collections.Generic;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Presenters;

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
        private FakeSelectMenuView _selectView;
        private SelectMenuPresenter _select;
        private MainMenuPresenter _main;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _world = new EcsWorld();
            var config = _objects.Config();
            config.DefaultUnlockedThemes = new[] { "cities" };
            _cities = config.Themes[0];
            _cities.Puzzles = _objects.Puzzles("cities", "One");
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
            var progress = new PlayerProgressService(save);
            _session = new GameSession(config);
            _energy = new EnergyService(config, save, new FakeClock { UtcNow = DateTime.UtcNow });
            _start = new GameStartService(_session, _energy, _world, new FakeSpriteLoader(_objects));
            _selectView = new FakeSelectMenuView();
            var loader = new FakeSpriteLoader(_objects);
            _select = new SelectMenuPresenter(config, _start, new ThemeShop(new StarService(save), progress, _world), progress, _session, new FakeHeaderPanelView(), _world, new FakeTexts(), loader, loader.Previews(config));
            _select.Initialize(_selectView);
            _main = new MainMenuPresenter(config, progress, _start, _session, new FakeHeaderPanelView(), _world, new FakeTexts(), loader.Previews(config));
            _main.Initialize(new FakeMainMenuView());
        }

        [TearDown]
        public void TearDown()
        {
            _world.Destroy();
            _objects.Dispose();
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

            Assert.That(_energy.GetBalance(), Is.EqualTo(4));
            Assert.That(_session.IsRunning, Is.True);
        }

        [Test]
        public void Start_WaitsForTheHideAnimation_ThenNavigatesOnce()
        {
            StartFromBothMenus();
            Assert.That(_world.Count<ChangeStateEvent>(), Is.Zero, "no navigation before the animation completes");

            _selectView.Hide.TrySetResult();
            Assert.That(_world.Count<ChangeStateEvent>(), Is.EqualTo(1));
        }

        [Test]
        public void NextRun_CanStart_ButNotWithoutEnergy()
        {
            StartFromBothMenus();
            _session.EndRun();
            Assert.That(_start.TryStart(_cities, _cities.Puzzles[0]).GetAwaiter().GetResult(), Is.True);
            Assert.That(_energy.GetBalance(), Is.EqualTo(3));

            _session.EndRun();
            _energy.Spend(_energy.GetBalance());
            Assert.That(_start.TryStart(_cities, _cities.Puzzles[0]).GetAwaiter().GetResult(), Is.False);
            Assert.That(_session.IsRunning, Is.False);
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
            var save = new PlayerDataSaveHelper(new MemoryStorage(), _config);
            save.GetPlayerData().PlayerProgress.CompletedPuzzles.Add("dogs.corgi");
            _session = new GameSession(_config);
            var energy = new EnergyService(_config, save, new FakeClock { UtcNow = DateTime.UtcNow });
            _view = new FakeMainMenuView();
            _header = new FakeHeaderPanelView();
            _sprites = new FakeSpriteLoader(_objects);
            _menu = new MainMenuPresenter(_config, new PlayerProgressService(save), new GameStartService(_session, energy, _world, _sprites), _session, _header, _world, new FakeTexts(), _sprites.Previews(_config));
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

            Assert.That(_session.IsRunning, Is.False);
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
