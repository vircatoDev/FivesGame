using System.Collections.Generic;
using Fives.Configs;
using Fives.Models;
using Fives.Tests.Support;
using Fives.UI;
using Fives.UI.Navigation;
using NUnit.Framework;
using UnityEngine;
using VContainer;

namespace Fives.Runtime.Tests.Screens
{
    /// <summary>
    /// ScreenNavigator на настоящем контейнере VContainer: экран открывается на своём слое,
    /// презентер берётся по типу View.
    /// </summary>
    public sealed class ScreenNavigatorTests
    {
        private TestObjects _objects;
        private Transform _screens;
        private Transform _popups;
        private IScreenNavigator _navigator;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _screens = _objects.Rect("Screens");
            _popups = _objects.Rect("Popups");
            var template = _objects.Rect("Screen").gameObject.AddComponent<TestScreenView>();
            var configs = new Dictionary<GameStateType, StateConfig>
            {
                [GameStateType.MainMenu] = Config(GameStateType.MainMenu, template, false),
                [GameStateType.Settings] = Config(GameStateType.Settings, template, true)
            };

            // Регистрация как в GameLifetimeScope.
            var builder = new ContainerBuilder();
            builder.Register<IReadOnlyDictionary<GameStateType, StateConfig>>(_ => configs, Lifetime.Singleton);
            builder.RegisterInstance(new ScreenLayers(_screens, _popups));
            builder.Register<ScreenNavigator>(Lifetime.Singleton).As<IScreenNavigator>();
            builder.Register<TestScreenPresenter>(Lifetime.Transient);
            _navigator = builder.Build().Resolve<IScreenNavigator>();
        }

        [TearDown]
        public void TearDown() => _objects.Dispose(); // слои уничтожаются вместе с открытыми экранами

        private StateConfig Config(GameStateType state, TestScreenView prefab, bool popup)
        {
            var config = _objects.Asset<StateConfig>();
            config.StateName = state;
            config.ScreenPrefab = prefab;
            config.IsPopup = popup;
            return config;
        }

        private static TestScreenPresenter PresenterOf(Transform screen) => screen.GetComponent<TestScreenView>().Current;

        /// <summary>Экран открывается на слое экранов, и к нему подключается презентер по типу View.</summary>
        [Test]
        public void AScreen_OpensOnItsLayer_WithAPresenterForTheViewsType()
        {
            _navigator.Open(GameStateType.MainMenu);

            Assert.That(_screens.childCount, Is.EqualTo(1));
            Assert.That(PresenterOf(_screens.GetChild(0)).Activations, Is.EqualTo(1));
        }

        /// <summary>Попап открывается на слое попапов, а не экранов.</summary>
        [Test]
        public void APopup_OpensOnThePopupLayer()
        {
            _navigator.Open(GameStateType.Settings);

            Assert.That(_popups.childCount, Is.EqualTo(1));
            Assert.That(_screens.childCount, Is.Zero);
        }

        /// <summary>Уже открытый экран второй раз не открывается.</summary>
        [Test]
        public void AnOpenScreen_IsNotOpenedTwice()
        {
            _navigator.Open(GameStateType.MainMenu);
            _navigator.Open(GameStateType.MainMenu);

            Assert.That(_screens.childCount, Is.EqualTo(1));
        }
    }
}
