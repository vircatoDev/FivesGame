using NUnit.Framework;
using Scripts.Configs;
using Scripts.UI;

namespace Fives.Runtime.Tests
{
    public sealed class ThemeDownloadGateTests
    {
        private TestObjects _objects;
        private ThemeConfig _theme;
        private FakeThemeDownloads _downloads;
        private FakeDownloadScreen _screen;
        private ThemeDownloadGate _gate;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _theme = _objects.Config().Themes[0];
            _downloads = new FakeThemeDownloads();
            _screen = new FakeDownloadScreen();
            _gate = new ThemeDownloadGate(_downloads, _screen);
        }

        [TearDown]
        public void TearDown() => _objects.Dispose();

        private bool Ensure() => _gate.Ensure(_theme, default).GetAwaiter().GetResult();

        [Test]
        public void ADownloadedTheme_PassesWithoutTheLoadingScreen()
        {
            _downloads.OnDevice.Add(_theme.Id);

            Assert.That(Ensure(), Is.True);
            Assert.That(_downloads.Downloads, Is.Zero);
            Assert.That(_screen.Progress, Is.Zero, "the loading screen never showed");
        }

        [Test]
        public void AMissingTheme_DownloadsWithProgress_ThenTheScreenHides()
        {
            Assert.That(Ensure(), Is.True);
            Assert.That(_downloads.Downloads, Is.EqualTo(1));
            Assert.That(_screen.Progress, Is.EqualTo(1f));
            Assert.That(_screen.Shown, Is.False);
        }

        [Test]
        public void AFailedDownload_IsRetriedUntilItWorks()
        {
            _downloads.Failures = 2;
            _screen.Answers.Enqueue(true);
            _screen.Answers.Enqueue(true);

            Assert.That(Ensure(), Is.True);
            Assert.That(_downloads.Downloads, Is.EqualTo(3));
            Assert.That(_screen.Asked, Is.EqualTo(2));
            Assert.That(_downloads.OnDevice, Does.Contain(_theme.Id));
        }

        [Test]
        public void GivingUp_LeavesTheThemeMissing_AndHidesTheScreen()
        {
            _downloads.Failures = 1;
            _screen.Answers.Enqueue(false);

            Assert.That(Ensure(), Is.False);
            Assert.That(_downloads.OnDevice, Is.Empty);
            Assert.That(_screen.Shown, Is.False);
        }
    }
}
