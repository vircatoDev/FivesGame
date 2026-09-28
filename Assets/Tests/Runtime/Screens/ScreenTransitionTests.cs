using System.Collections.Generic;
using System.Linq;
using Fives.Configs;
using Fives.Models;
using Fives.UI;
using Fives.UI.Navigation;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Screens
{
    /// <summary>
    /// Какие экраны открывает и закрывает машина состояний: попап открывается поверх экрана, другой экран заменяет всё.
    /// </summary>
    public sealed class ScreenTransitionTests
    {
        private sealed class FakeNavigator : IScreenNavigator
        {
            public readonly List<string> Calls = new List<string>();
            public void Open(GameStateType state) => Calls.Add("open " + state);
            public void Close(GameStateType state) => Calls.Add("close " + state);
            public void CloseAll() => Calls.Add("close all");
        }

        private TestObjects _objects;
        private FakeNavigator _navigator;
        private GameStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _navigator = new FakeNavigator();
            var popups = new[] { GameStateType.Finished, GameStateType.Settings };
            var configs = System.Enum.GetValues(typeof(GameStateType)).Cast<GameStateType>().ToDictionary(state => state, state =>
            {
                var config = _objects.Asset<StateConfig>();
                config.StateName = state;
                config.IsPopup = popups.Contains(state);
                return config;
            });
            _machine = new GameStateMachine(configs, _navigator);
        }

        [TearDown]
        public void TearDown() => _objects.Dispose();

        private string[] Go(params GameStateType[] states)
        {
            _navigator.Calls.Clear();
            foreach (var state in states)
                _machine.ChangeState(state);
            return _navigator.Calls.ToArray();
        }

        /// <summary>Первое состояние просто открывает свой экран.</summary>
        [Test]
        public void TheFirstState_OpensItsScreen()
        {
            Assert.That(Go(GameStateType.MainMenu), Is.EqualTo(new[] { "open MainMenu" }));
        }

        /// <summary>Попап настроек открывается поверх меню, меню не закрывается.</summary>
        [Test]
        public void APopup_OpensOverTheScreen()
        {
            Go(GameStateType.MainMenu);

            Assert.That(Go(GameStateType.Settings), Is.EqualTo(new[] { "open Settings" }));
        }

        /// <summary>Из попапа назад в меню: закрывается только попап.</summary>
        [Test]
        public void LeavingAPopup_ForTheScreenBelow_ClosesOnlyThePopup()
        {
            Go(GameStateType.MainMenu, GameStateType.Settings);

            Assert.That(Go(GameStateType.MainMenu), Is.EqualTo(new[] { "close Settings", "open MainMenu" }));
        }

        /// <summary>Переход на другой экран сначала закрывает всё, потом открывает новый.</summary>
        [Test]
        public void AnotherScreen_ReplacesEverything_BeforeItOpens()
        {
            Go(GameStateType.MainMenu);

            Assert.That(Go(GameStateType.SelectMenu), Is.EqualTo(new[] { "close all", "open SelectMenu" }));
        }

        /// <summary>Back с экрана тем в меню закрывает только экран тем.</summary>
        [Test]
        public void BackFromTheThemeScreen_ClosesOnlyIt()
        {
            Go(GameStateType.MainMenu, GameStateType.SelectMenu);

            Assert.That(Go(GameStateType.MainMenu), Is.EqualTo(new[] { "close SelectMenu", "open MainMenu" }));
        }

        /// <summary>
        /// Окно результата открывается поверх партии;
        /// переход из него в меню закрывает и окно, и партию.
        /// </summary>
        [Test]
        public void TheResultOverTheGame_ThenTheMenu_ClosesBoth()
        {
            Go(GameStateType.MainMenu, GameStateType.Playing);

            Assert.That(Go(GameStateType.Finished), Is.EqualTo(new[] { "open Finished" }));
            Assert.That(Go(GameStateType.MainMenu), Is.EqualTo(new[] { "close all", "open MainMenu" }));
        }
    }
}
