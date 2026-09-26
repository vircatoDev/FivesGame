using System;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Presenters;

namespace Fives.Runtime.Tests
{
    public sealed class RewardedAdTests
    {
        private TestObjects _objects;
        private EcsWorld _world;
        private StarService _stars;
        private FakeRewardedAds _ads;
        private FakeGameResultView _view;
        private GameResultPresenter _result;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _world = new EcsWorld();
            var config = _objects.Config();
            config.Themes[1].Puzzles = _objects.Puzzles("dogs", "Rex");
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            _stars = new StarService(save);
            var session = new GameSession(new GameBalance(config));
            session.SetSelectedTheme(config.Themes[1]);
            session.SetSelectedImage(config.Themes[1].Puzzles[0], null);
            session.BeginRun();
            _ads = new FakeRewardedAds();
            _view = new FakeGameResultView();
            _result = new GameResultPresenter(session, _stars, new PlayerProgressService(save), _world, new FakeTexts(), _ads);
        }

        [TearDown]
        public void TearDown()
        {
            _world.Destroy();
            _objects.Dispose();
        }

        [Test]
        public void WithoutAds_TheDoubleRewardIsHidden()
        {
            _ads.IsSupported = false;
            _result.Initialize(_view);

            Assert.That(_view.DoubleOffered, Is.False);
        }

        [Test]
        public void TheButton_FollowsTheLoadedAd_UntilTheScreenCloses()
        {
            _ads.SetReady(false);
            _result.Initialize(_view);
            Assert.That((_view.DoubleOffered, _view.DoubleReady), Is.EqualTo((true, false)));

            _ads.SetReady(true);
            Assert.That(_view.DoubleReady, Is.True);

            _view.Destroy(_result);
            Assert.DoesNotThrow(() => _ads.SetReady(false), "a closed screen no longer listens");
        }

        [Test]
        public void AWatchedAd_DoublesTheReward()
        {
            _result.Initialize(_view);
            _result.GetDoubleReward();

            Assert.That(_ads.Shown, Is.EqualTo(1));
            Assert.That(_stars.GetBalance(), Is.EqualTo(220));
            Assert.That(_world.Count<Scripts.Components.ChangeStateEvent>(), Is.EqualTo(1), "back to the main menu");
        }

        [Test]
        public void ASkippedAd_GrantsNothing_AndThePlainRewardStays()
        {
            _ads.Watched = false;
            _result.Initialize(_view);
            _result.GetDoubleReward();
            Assert.That(_stars.GetBalance(), Is.EqualTo(200));

            _result.GetReward();
            Assert.That(_stars.GetBalance(), Is.EqualTo(210));
        }
    }
}
