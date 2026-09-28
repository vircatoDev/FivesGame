using Fives.Components;
using Fives.Configs;
using Fives.UI.Presenters;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Economy
{
    /// <summary>
    /// Окно результата: награда, удвоенная за досмотренную рекламу и выданная один раз, и прогресс темы.
    /// </summary>
    public sealed class ResultScreenTests
    {
        private GameStand _game;
        private ThemeConfig _dogs;
        private FakeGameResultView _view;
        private GameResultPresenter _result;

        [SetUp]
        public void SetUp()
        {
            _game = new GameStand();
            _dogs = _game.Theme("dogs");
            _dogs.Puzzles = _game.Objects.Puzzles("dogs", "Rex");
            _game.BeginRun(_dogs, _dogs.Puzzles[0]);
            _view = new FakeGameResultView();
            _result = _game.Result();
        }

        [TearDown]
        public void TearDown() => _game.Dispose();

        private int Stars => _game.Stars.GetBalance();

        /// <summary>Нажаты обе кнопки награды: звёзды начислены один раз, обычная награда.</summary>
        [Test]
        public void TheReward_IsPaidOnce_EvenIfBothButtonsArePressed()
        {
            _result.Initialize(_view);
            _result.GetReward();
            _result.GetDoubleReward();

            Assert.That(Stars, Is.EqualTo(TestConfig.Stars + TestConfig.RewardStars));
        }

        /// <summary>Досмотренная реклама удваивает награду, после чего игрок возвращается в главное меню.</summary>
        [Test]
        public void AWatchedAd_DoublesTheReward()
        {
            _result.Initialize(_view);
            _result.GetDoubleReward();

            Assert.That(_game.Ads.Shown, Is.EqualTo(1));
            Assert.That(Stars, Is.EqualTo(TestConfig.Stars + 2 * TestConfig.RewardStars));
            Assert.That(_game.World.Count<ChangeStateEvent>(), Is.EqualTo(1), "back to the main menu");
        }

        /// <summary>Реклама не досмотрена: x2 ничего не даёт, обычная награда остаётся доступной.</summary>
        [Test]
        public void ASkippedAd_GrantsNothing_AndThePlainRewardStays()
        {
            _game.Ads.Watched = false;
            _result.Initialize(_view);
            _result.GetDoubleReward();
            Assert.That(Stars, Is.EqualTo(TestConfig.Stars));

            _result.GetReward();
            Assert.That(Stars, Is.EqualTo(TestConfig.Stars + TestConfig.RewardStars));
        }

        /// <summary>Если реклама на платформе не поддерживается, кнопки x2 нет.</summary>
        [Test]
        public void WithoutAds_TheDoubleRewardIsHidden()
        {
            _game.Ads.IsSupported = false;
            _result.Initialize(_view);

            Assert.That(_view.DoubleOffered, Is.False);
        }

        /// <summary>
        /// Кнопка x2 активна, только когда реклама загружена;
        /// закрытый экран больше не следит за рекламой.
        /// </summary>
        [Test]
        public void TheButton_FollowsTheLoadedAd_UntilTheScreenCloses()
        {
            _game.Ads.SetReady(false);
            _result.Initialize(_view);
            Assert.That((_view.DoubleOffered, _view.DoubleReady), Is.EqualTo((true, false)));

            _game.Ads.SetReady(true);
            Assert.That(_view.DoubleReady, Is.True);

            _view.Destroy(_result);
            Assert.DoesNotThrow(() => _game.Ads.SetReady(false), "a closed screen no longer listens");
        }

        /// <summary>
        /// Прогресс темы считает собранные пазлы (1/3), а не номер текущего;
        /// сам экран партию не засчитывает и не сохраняет.
        /// </summary>
        [Test]
        public void Progress_CountsSolvedPuzzles_AndTheScreenDoesNotRecordTheRun()
        {
            _dogs.Puzzles = _game.Objects.Puzzles("dogs", "A", "B", "C");
            _game.Progress.MarkCompleted(_dogs.Puzzles[0]); // собран раньше
            _game.BeginRun(_dogs, _dogs.Puzzles[2]);        // только что сыгран третий пазл

            _result.Initialize(_view);

            Assert.That(_view.Progress, Is.EqualTo("1/3"), "puzzles solved, not the position of this one");
            Assert.That(_game.Progress.IsCompleted(_dogs.Puzzles[2]), Is.False, "the core records a solved puzzle, not the result screen");
            Assert.That(_game.World.Count<SaveDataEvent>(), Is.Zero);
        }
    }
}
