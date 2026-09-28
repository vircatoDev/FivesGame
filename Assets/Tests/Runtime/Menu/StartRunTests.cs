using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Fives.Components;
using Fives.Configs;
using Fives.Services;
using Fives.Systems;
using Fives.UI.Presenters;
using Leopotam.Ecs;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fives.Runtime.Tests.Menu
{
    /// <summary>
    /// Старт партии из главного меню и с экрана темы: один старт на тап, энергия списывается один раз и только
    /// после того, как меню спряталось; отменённый старт или незагрузившаяся картинка ничего не стоят.
    /// </summary>
    public sealed class StartRunTests
    {
        private GameStand _game;
        private ThemeConfig _cities;
        private ThemeConfig _dogs;
        private FakeSelectMenuView _selectView;
        private SelectMenuPresenter _select;
        private FakeMainMenuView _mainView;
        private MainMenuPresenter _main;

        [SetUp]
        public void SetUp()
        {
            _game = new GameStand();
            _game.Config.DefaultUnlockedThemes = new[] { "cities" };
            _cities = _game.Theme("cities");
            _cities.Puzzles = _game.Objects.Puzzles("cities", "One");
            _dogs = _game.Theme("dogs");
            _dogs.Puzzles = _game.Objects.Puzzles("dogs", "Rex");
            _selectView = new FakeSelectMenuView();
            _select = _game.SelectMenu(_selectView);
            _mainView = new FakeMainMenuView();
            _main = _game.MainMenu(_mainView);
        }

        [TearDown]
        public void TearDown() => _game.Dispose();

        private int Energy => _game.Energy.GetBalance();
        private int Requests => _game.World.Count<StartRunRequest>();

        // Партия заканчивается так же, как в игре: GameEndEvent через BoardDestroySystem.
        private void EndRun()
        {
            var systems = new EcsSystems(_game.World).Add(new BoardDestroySystem()).OneFrame<GameEndEvent>();
            systems.Init();
            _game.World.Send<GameEndEvent>();
            systems.Run();
            systems.Destroy();
        }

        private bool StartDirectly()
        {
            if (!_game.Start.Prepare(_cities, _cities.Puzzles[0], CancellationToken.None).GetAwaiter().GetResult())
                return false;
            _game.Start.Begin();
            return true;
        }

        private void StartFromBothMenus()
        {
            _selectView.OnClick("cities");       // карточка темы → список пазлов
            _selectView.OnClick("cities.one");   // карточка пазла, два тапа
            _selectView.OnClick("cities.one");
            _main.OnStartGame();
        }

        /// <summary>
        /// Старт нажат трижды:
        /// дважды по пазлу на экране темы и «Играть» в меню.
        /// Партия одна, энергия списана один раз.
        /// </summary>
        [Test]
        public void Start_IsAcceptedOnceAcrossBothMenus()
        {
            StartFromBothMenus();
            _selectView.Hide.TrySetResult();
            _mainView.Hide.TrySetResult();

            Assert.That(Energy, Is.EqualTo(TestConfig.Energy - 1));
            Assert.That(Requests, Is.EqualTo(1), "one board is requested");
        }

        /// <summary>
        /// Пока меню не спряталось, энергия не списывается и экран не меняется;
        /// после анимации одно списание и один переход.
        /// </summary>
        [Test]
        public void Start_WaitsForTheHideAnimation_ThenPaysAndNavigatesOnce()
        {
            StartFromBothMenus();
            Assert.That(_game.World.Count<ChangeStateEvent>(), Is.Zero, "no navigation before the animation completes");
            Assert.That(Energy, Is.EqualTo(TestConfig.Energy), "nothing is paid before the animation completes");

            _selectView.Hide.TrySetResult();
            Assert.That(_game.World.Count<ChangeStateEvent>(), Is.EqualTo(1));
            Assert.That(Energy, Is.EqualTo(TestConfig.Energy - 1));
        }

        /// <summary>После конца партии можно начать следующую; без энергии старт не проходит.</summary>
        [Test]
        public void NextRun_CanStart_ButNotWithoutEnergy()
        {
            StartFromBothMenus();
            _selectView.Hide.TrySetResult();
            EndRun();
            Assert.That(StartDirectly(), Is.True);
            Assert.That(Energy, Is.EqualTo(TestConfig.Energy - 2));

            EndRun();
            _game.Energy.Spend(Energy);
            Assert.That(StartDirectly(), Is.False);
            Assert.That(Requests, Is.Zero);
        }

        /// <summary>Begin без Prepare — ошибка в коде, она не проходит молча.</summary>
        [Test]
        public void Begin_WithoutPrepare_IsAProgrammingError()
        {
            Assert.Throws<System.InvalidOperationException>(() => _game.Start.Begin());
        }

        /// <summary>
        /// Меню закрыли, пока грузилась картинка пазла:
        /// энергия не списана, партия не началась, следующий старт работает.
        /// </summary>
        [Test]
        public void MenuClosedWhileThePictureLoads_CostsNothing_AndTheNextStartWorks()
        {
            _game.Sprites.Slow = true;
            _main.OnStartGame();
            _mainView.Destroy(_main);
            _game.Sprites.FinishLoads();

            Assert.That(Energy, Is.EqualTo(TestConfig.Energy));
            Assert.That(Requests, Is.Zero);
            Assert.That(_game.World.Count<ChangeStateEvent>(), Is.Zero);

            _game.Sprites.Slow = false;
            Assert.That(StartDirectly(), Is.True, "the cancelled start holds nothing back");
        }

        /// <summary>Экран темы закрыли во время его анимации: ничего не списано, следующий старт работает.</summary>
        [Test]
        public void MenuClosedDuringItsHideAnimation_CostsNothing_AndTheNextStartWorks()
        {
            _selectView.OnClick("cities");
            _selectView.OnClick("cities.one");
            _selectView.Destroy(_select);

            Assert.That(Energy, Is.EqualTo(TestConfig.Energy));
            Assert.That(Requests, Is.Zero);
            Assert.That(StartDirectly(), Is.True, "a preparation from a closed screen holds nothing back");
        }

        /// <summary>
        /// Картинка пазла не загрузилась:
        /// энергия не списана, партия не началась, следующий старт работает.
        /// </summary>
        [Test]
        public void FailedLoad_CostsNothing_AndTheNextStartWorks()
        {
            _game.Sprites.Failing = true;
            LogAssert.Expect(LogType.Exception, new Regex("The bundle is missing"));
            _main.OnStartGame();

            Assert.That(Energy, Is.EqualTo(TestConfig.Energy));
            Assert.That(Requests, Is.Zero);

            _game.Sprites.Failing = false;
            Assert.That(StartDirectly(), Is.True);
        }

        /// <summary>Back, пока грузятся пазлы темы: экран остаётся на списке тем.</summary>
        [Test]
        public void BackWhileThePuzzlesLoad_KeepsTheThemeList()
        {
            _game.Sprites.Slow = true;
            _selectView.OnClick("cities");
            _select.OnExit(); // Back в шапке
            _game.Sprites.FinishLoads();

            Assert.That(_selectView.Items.Select(item => item.Id), Does.Contain("cities").And.Not.Contain("cities.one"));
        }

        /// <summary>Два тапа по разным темам подряд: показываются пазлы последней.</summary>
        [Test]
        public void TheLatestThemeTap_Wins()
        {
            _game.Progress.Unlock(_dogs);
            _game.Sprites.Slow = true;
            var themes = _selectView.OnClick;
            themes("cities");
            themes("dogs");
            _game.Sprites.FinishLoads();

            Assert.That(_selectView.Items.Select(item => item.Id), Is.EqualTo(new[] { "dogs.rex" }));
        }

        /// <summary>Закрытый экран темы освобождает загруженные им картинки; остаются только превью меню.</summary>
        [Test]
        public void ClosedScreen_ReleasesThePicturesItLoaded()
        {
            _game.Sprites.Slow = true;
            _selectView.OnClick("cities");
            _selectView.Destroy(_select);
            _game.Sprites.FinishLoads();

            Assert.That(_game.Sprites.Held.Keys.All(owner => owner is ThemePreviews), Is.True, "only the menu previews stay loaded");
        }
    }
}
