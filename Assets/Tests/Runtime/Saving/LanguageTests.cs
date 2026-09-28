using Fives.Models;
using Fives.Services;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Saving
{
    /// <summary>
    /// Выбранный язык: восстанавливается из сохранения, проверяется по списку поддерживаемых и сохраняется.
    /// </summary>
    public sealed class LanguageTests
    {
        private GameStand _game;

        [SetUp]
        public void SetUp() => _game = new GameStand();

        [TearDown]
        public void TearDown() => _game.Dispose();

        private LanguageService Service() => new LanguageService(_game.Config, _game.Save);

        /// <summary>Язык из сохранения восстанавливается.</summary>
        [Test]
        public void SavedLanguage_IsRestored()
        {
            _game.Save.GetPlayerData().Language = "de";

            Assert.That(Service().Current, Is.EqualTo("de"));
        }

        /// <summary>Язык не сохранён или неизвестен: выбирается один из поддерживаемых.</summary>
        [TestCase(null)]  // язык не сохранён
        [TestCase("")]    // пустая строка
        [TestCase("xx")]  // неизвестный код
        public void MissingOrUnknownSave_FallsBackToASupportedLanguage(string saved)
        {
            _game.Save.GetPlayerData().Language = saved;

            Assert.That(Service().Supported, Does.Contain(Service().Current));
        }

        /// <summary>
        /// Неизвестный или уже выбранный язык выбрать нельзя;
        /// другой поддерживаемый выбирается и попадает в сохранение.
        /// </summary>
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

        /// <summary>
        /// Сервис языка создаётся раньше, чем загружено сохранение (так его получает экран загрузки игры),
        /// и берёт сохранённый язык, когда тот понадобится.
        /// </summary>
        [Test]
        public void Service_CreatedBeforeTheSaveLoads_ReadsTheSavedLanguageLater()
        {
            _game.Save.GetPlayerData().Language = "de";
            _game.Save.SaveAll();
            var save = new PlayerSave(_game.Storage, _game.Config, _game.Balance);
            var language = new LanguageService(_game.Config, save);

            save.Load();

            Assert.That(language.Current, Is.EqualTo("de"));
        }
    }
}
