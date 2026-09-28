using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Boot
{
    /// <summary>
    /// Темы скачиваются на экране загрузки; если скачать не вышло, загрузка ждёт, пока игрок нажмёт «Повторить».
    /// </summary>
    public sealed class ThemeDownloadTests
    {
        private GameStand _game;
        private FakeThemeDownloads _downloads;
        private FakeLoadingScreen _screen;

        [SetUp]
        public void SetUp()
        {
            _game = new GameStand();
            _downloads = new FakeThemeDownloads();
            _screen = new FakeLoadingScreen();
        }

        [TearDown]
        public void TearDown() => _game.Dispose();

        /// <summary>
        /// Скачать темы не вышло два раза: экран загрузки дважды просит повторить, и загрузка идёт дальше, только
        /// когда все темы на устройстве.
        /// </summary>
        [Test]
        public void FailedDownload_WaitsForRetry_UntilEveryThemeIsOnTheDevice()
        {
            _downloads.Failures = 2;

            var step = _game.Boot(_downloads, _screen).DownloadThemes(null, CancellationToken.None);

            Assert.That(step.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_screen.Retries, Is.EqualTo(2));
            Assert.That(_downloads.OnDevice, Is.EquivalentTo(_game.Config.Themes.Select(theme => theme.Id)));
        }

        /// <summary>Игру закрыли, пока экран загрузки ждал «Повторить»: загрузка останавливается и больше не скачивает.</summary>
        [Test]
        public void ClosingTheGame_WhileWaitingForRetry_StopsTheBoot()
        {
            _downloads.Failures = 1;
            _screen.Stuck = true;
            using var closing = new CancellationTokenSource();

            var step = _game.Boot(_downloads, _screen).DownloadThemes(null, closing.Token);
            closing.Cancel();

            Assert.That(step.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(_downloads.Downloads, Is.EqualTo(1));
        }
    }
}
