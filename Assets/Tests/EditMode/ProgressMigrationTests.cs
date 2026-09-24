using NUnit.Framework;

namespace Fives.Domain.Tests
{
    public sealed class ProgressMigrationTests
    {
        private static readonly (string Name, string Id)[] Catalog =
        {
            ("Dogs", "dogs"), ("Corgi", "dogs.corgi"), ("Pug", "dogs.pug"), ("Twin", "a.twin"), ("Twin", "b.twin")
        };

        [Test]
        public void NamesBecomeIds()
        {
            Assert.That(ProgressMigration.NamesToIds(new[] { "Dogs", "Corgi" }, Catalog), Is.EqualTo(new[] { "dogs", "dogs.corgi" }));
        }

        [Test]
        public void UnknownEntriesAreKeptAndEmptyOnesDropped()
        {
            Assert.That(ProgressMigration.NamesToIds(new[] { "Cities", null, "", "Pug" }, Catalog),
                Is.EqualTo(new[] { "Cities", "dogs.pug" }));
        }

        [Test]
        public void SharedNameMapsToEveryIdOnceAndMigrationIsIdempotent()
        {
            var once = ProgressMigration.NamesToIds(new[] { "Twin", "Twin" }, Catalog);
            Assert.That(once, Is.EqualTo(new[] { "a.twin", "b.twin" }));
            Assert.That(ProgressMigration.NamesToIds(once, Catalog), Is.EqualTo(once));
        }
    }
}
