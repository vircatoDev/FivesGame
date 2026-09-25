using NUnit.Framework;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;

namespace Fives.Runtime.Tests
{
    public sealed class LanguageTests
    {
        private TestObjects _objects;
        private PlayerDataSaveHelper _save;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _save = new PlayerDataSaveHelper(new MemoryStorage(), _objects.Config());
        }

        [TearDown]
        public void TearDown() => _objects.Dispose();

        private LanguageService Service() => new LanguageService(_objects.Config(), _save);

        [Test]
        public void SavedLanguage_IsRestored()
        {
            _save.GetPlayerData().Language = "de";

            Assert.That(Service().Current, Is.EqualTo("de"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("xx")]
        public void MissingOrUnknownSave_FallsBackToASupportedLanguage(string saved)
        {
            _save.GetPlayerData().Language = saved;

            Assert.That(Service().Supported, Does.Contain(Service().Current));
        }

        [Test]
        public void Choice_IsValidated_AndWrittenToTheSave()
        {
            var language = Service();
            var other = language.Current == "fr" ? "it" : "fr";

            Assert.That(language.TrySet("xx"), Is.False);
            Assert.That(language.TrySet(language.Current), Is.False);
            Assert.That(language.TrySet(other), Is.True);

            var data = new GameSaveData();
            language.UpdatePlayerData(data);
            Assert.That(data.Language, Is.EqualTo(other));
        }
    }
}
