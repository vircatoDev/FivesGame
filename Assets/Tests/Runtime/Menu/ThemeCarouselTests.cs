using Fives.Components;
using Fives.Models;
using Fives.UI.Presenters;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Menu
{
    /// <summary>
    /// Карусель тем в главном меню: на какой теме открывается, что показывают карточки, куда ведёт «Играть».
    /// </summary>
    public sealed class ThemeCarouselTests
    {
        private GameStand _game;
        private FakeMainMenuView _view;
        private MainMenuPresenter _menu;

        [SetUp]
        public void SetUp()
        {
            _game = new GameStand();
            var objects = _game.Objects;
            _game.Theme("cities").Puzzles = objects.Puzzles("cities", "Paris");
            _game.Theme("dogs").Puzzles = objects.Puzzles("dogs", "Corgi", "Pug", "Shepherd");
            _game.Config.Themes.Add(objects.Theme("cats", "Cats", 60, objects.Puzzles("cats", "Siamese", "Ginger")));
            _game.Config.DefaultUnlockedThemes = new[] { "dogs", "cats" };
            _game.Save.GetPlayerData().PlayerProgress.CompletedPuzzles.Add("dogs.corgi");
            _view = new FakeMainMenuView();
            _menu = _game.MainMenu(_view);
            _view.Hide.TrySetResult();
        }

        [TearDown]
        public void TearDown() => _game.Dispose();

        /// <summary>В главном меню кнопка шапки — «Настройки», и она открывает попап настроек.</summary>
        [Test]
        public void HeaderButton_IsSettings_AndOpensTheSettingsScreen()
        {
            Assert.That(_game.Header.Button, Is.EqualTo(HeaderBtnType.Settings));

            _game.Header.OnButton();
            Assert.That(_game.World.Count<ChangeStateEvent>(), Is.EqualTo(1));
        }

        /// <summary>Меню открывается на последней открытой теме (cats) с её превью.</summary>
        [Test]
        public void Opens_OnTheLastUnlockedTheme_WithItsPreview()
        {
            Assert.That(_view.Current.TitleKey, Is.EqualTo("theme.cats"));
            Assert.That(_view.Current.Image, Is.EqualTo(_game.Sprites.Of(_game.Theme("cats").Preview)));
            Assert.That(_view.CanBrowse, Is.True);
            Assert.That(_view.Direction, Is.Zero);
        }

        /// <summary>В карусели все темы, закрытые тоже; у темы без собранных пазлов прогресс «0/1».</summary>
        [Test]
        public void ListsEveryTheme_LockedOnesIncluded_UntouchedAtZero()
        {
            Assert.That(_view.Previous.TitleKey, Is.EqualTo("theme.dogs"));
            Assert.That(_view.Next.TitleKey, Is.EqualTo("theme.cities"));
            Assert.That(_view.Next.Progress, Is.EqualTo("0/1"));
            Assert.That(_view.Next.Image, Is.EqualTo(_game.Sprites.Of(_game.Theme("cities").Preview)));
        }

        /// <summary>«Играть» на закрытой теме не начинает партию, а открывает экран тем на этой теме.</summary>
        [Test]
        public void PlayOnALockedTheme_OpensTheThemeScreenFocusedOnIt()
        {
            _menu.OnNextTheme();
            _menu.OnStartGame();

            Assert.That(_game.World.Count<StartRunRequest>(), Is.Zero, "a locked theme does not start");
            Assert.That(_game.Session.SelectedTheme, Is.EqualTo(_game.Theme("cities")));
            Assert.That(_game.World.Count<ChangeStateEvent>(), Is.EqualTo(1));
        }

        /// <summary>
        /// Карусель зациклена;
        /// «Играть» начинает следующий несобранный пазл темы в центре:
        /// Pug после собранного Corgi.
        /// </summary>
        [Test]
        public void Browsing_WrapsAround_AndPlayStartsTheCenteredThemesNextPuzzle()
        {
            _menu.OnNextTheme();
            _menu.OnNextTheme();

            var dogs = _game.Theme("dogs");
            Assert.That(_view.Current.TitleKey, Is.EqualTo("theme.dogs"));
            Assert.That(_view.Current.Image, Is.EqualTo(_game.Sprites.Of(dogs.Preview)));
            Assert.That(_view.Current.Progress, Is.EqualTo("1/3"));
            Assert.That(_view.Direction, Is.EqualTo(1));

            _menu.OnStartGame();
            Assert.That(_game.Session.SelectedTheme, Is.EqualTo(dogs));
            Assert.That(_game.Session.SelectedPuzzle, Is.EqualTo(dogs.Puzzles[1]));
        }
    }
}
