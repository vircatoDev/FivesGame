using NUnit.Framework;

namespace Fives.Domain.Tests
{
    /// <summary>Прогресс тем и перевод старых сохранений с имён на id.</summary>
    public sealed class ProgressTests
    {
        private static readonly (string Name, string Id)[] Catalog =
        {
            ("Dogs", "dogs"), ("Corgi", "dogs.corgi"), ("Pug", "dogs.pug"), ("Twin", "a.twin"), ("Twin", "b.twin")
        };

        /// <summary>
        /// Прогресс темы считает только её собранные пазлы:
        /// «1/2», собранный пазл другой темы не в счёт.
        /// </summary>
        [Test]
        public void ThemeProgress_CountsOnlyThisThemesPuzzles()
        {
            var progress = new ThemeProgress(new[] { "a", "b" }, new[] { "b", "other" });

            Assert.That(progress.ToString(), Is.EqualTo("1/2"));
            Assert.That(progress.IsComplete, Is.False);
            Assert.That(new ThemeProgress(new[] { "a" }, new[] { "a" }).IsComplete, Is.True);
        }

        /// <summary>Старое сохранение хранило имена тем и пазлов, миграция заменяет их на id.</summary>
        [Test]
        public void Migration_TurnsNamesIntoIds()
        {
            Assert.That(ProgressMigration.NamesToIds(new[] { "Dogs", "Corgi" }, Catalog), Is.EqualTo(new[] { "dogs", "dogs.corgi" }));
        }

        /// <summary>Неизвестные имена миграция оставляет как есть, пустые записи выбрасывает.</summary>
        [Test]
        public void Migration_KeepsUnknownEntries_AndDropsEmptyOnes()
        {
            Assert.That(ProgressMigration.NamesToIds(new[] { "Cities", null, "", "Pug" }, Catalog),
                Is.EqualTo(new[] { "Cities", "dogs.pug" }));
        }

        /// <summary>
        /// Имя, общее у двух пазлов, превращается в оба id по одному разу;
        /// повторная миграция ничего не меняет.
        /// </summary>
        [Test]
        public void Migration_MapsASharedNameToEveryIdOnce_AndCanRunTwice()
        {
            var once = ProgressMigration.NamesToIds(new[] { "Twin", "Twin" }, Catalog);
            Assert.That(once, Is.EqualTo(new[] { "a.twin", "b.twin" }));
            Assert.That(ProgressMigration.NamesToIds(once, Catalog), Is.EqualTo(once));
        }
    }
}
