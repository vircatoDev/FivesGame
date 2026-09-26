using System.Collections.Generic;
using NUnit.Framework;
using Scripts.Configs;
using Scripts.Helpers;

namespace Fives.Runtime.Tests
{
    public sealed class BalanceTests
    {
        private TestObjects _objects;
        private GlobalConfig _config;
        private GameBalance _balance;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _config = _objects.Config();
            _config.HintPrice = 5;
            _balance = new GameBalance(_config);
        }

        [TearDown]
        public void TearDown() => _objects.Dispose();

        [Test]
        public void WithoutRemoteConfig_TheBuiltInValuesApply()
        {
            Assert.That(_balance.HintPrice, Is.EqualTo(5));
            Assert.That(_balance.MaxEnergy, Is.EqualTo(10));
            Assert.That(_balance.PriceOf(_config.Themes[0]), Is.EqualTo(60));
        }

        [Test]
        public void AnOverride_ReplacesOnlyWhatItNames()
        {
            _balance.Apply(new BalanceOverrides { HintPrice = 3, ThemePrices = new Dictionary<string, int> { ["cities"] = 40 } });

            Assert.That(_balance.HintPrice, Is.EqualTo(3));
            Assert.That(_balance.PriceOf(_config.Themes[0]), Is.EqualTo(40));
            Assert.That(_balance.PriceOf(_config.Themes[1]), Is.EqualTo(0), "a theme the override does not name keeps its price");
            Assert.That(_balance.RewardStars, Is.EqualTo(10));
        }

        [Test]
        public void OutOfRangeOverrides_AreIgnored()
        {
            _balance.Apply(new BalanceOverrides
            {
                MaxEnergy = 0, EnergyRecoveryHours = -1, HintPrice = -2,
                ThemePrices = new Dictionary<string, int> { ["cities"] = -5 }
            });

            Assert.That(_balance.MaxEnergy, Is.EqualTo(10));
            Assert.That(_balance.EnergyRecoveryHours, Is.EqualTo(1f));
            Assert.That(_balance.HintPrice, Is.EqualTo(5));
            Assert.That(_balance.PriceOf(_config.Themes[0]), Is.EqualTo(60));
        }

        [Test]
        public void TheDashboardJson_IsRead()
        {
            _balance.Apply(BalanceOverrides.FromJson(
                "{ \"hintPrice\": 4, \"energyRecoveryHours\": 0.5, \"themePrices\": { \"cities\": 25 } }"));

            Assert.That(_balance.HintPrice, Is.EqualTo(4));
            Assert.That(_balance.EnergyRecoveryHours, Is.EqualTo(0.5f));
            Assert.That(_balance.PriceOf(_config.Themes[0]), Is.EqualTo(25));
        }

        [Test]
        public void ANewSave_StartsWithTheOverriddenBalance()
        {
            _balance.Apply(new BalanceOverrides { InitialStars = 50, InitialEnergy = 3 });

            var data = new PlayerDataSaveHelper(new MemoryStorage(), _config, _balance).GetPlayerData();

            Assert.That(data.Stars, Is.EqualTo(50));
            Assert.That(data.Energy.CurrentEnergy, Is.EqualTo(3));
        }
    }
}
