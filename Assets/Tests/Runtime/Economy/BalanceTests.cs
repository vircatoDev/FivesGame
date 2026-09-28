using System.Collections.Generic;
using Fives.Configs;
using Fives.Services;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Economy
{
    /// <summary>Цены и энергия из конфига и что из этого может поменять Remote Config.</summary>
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
            _balance = new GameBalance(_config);
        }

        [TearDown]
        public void TearDown() => _objects.Dispose();

        private ThemeConfig Cities => _config.Themes[0];
        private ThemeConfig Dogs => _config.Themes[1];

        /// <summary>Без Remote Config действуют значения из конфига игры.</summary>
        [Test]
        public void WithoutRemoteConfig_TheBuiltInValuesApply()
        {
            Assert.That(_balance.HintPrice, Is.EqualTo(TestConfig.HintPrice));
            Assert.That(_balance.MaxEnergy, Is.EqualTo(TestConfig.MaxEnergy));
            Assert.That(_balance.PriceOf(Cities), Is.EqualTo(TestConfig.CitiesPrice));
        }

        /// <summary>Remote Config меняет только названные значения, остальные остаются из конфига.</summary>
        [Test]
        public void AnOverride_ReplacesOnlyWhatItNames()
        {
            _balance.Apply(new BalanceOverrides { HintPrice = 3, ThemePrices = new Dictionary<string, int> { ["cities"] = 40 } });

            Assert.That(_balance.HintPrice, Is.EqualTo(3));
            Assert.That(_balance.PriceOf(Cities), Is.EqualTo(40));
            Assert.That(_balance.PriceOf(Dogs), Is.EqualTo(0), "a theme the override does not name keeps its price");
            Assert.That(_balance.RewardStars, Is.EqualTo(TestConfig.RewardStars));
        }

        /// <summary>Недопустимые значения из Remote Config, ноль и отрицательные, игнорируются.</summary>
        [Test]
        public void OutOfRangeOverrides_AreIgnored()
        {
            _balance.Apply(new BalanceOverrides
            {
                MaxEnergy = 0, EnergyRecoveryHours = -1, HintPrice = -2,
                ThemePrices = new Dictionary<string, int> { ["cities"] = -5 }
            });

            Assert.That(_balance.MaxEnergy, Is.EqualTo(TestConfig.MaxEnergy));
            Assert.That(_balance.EnergyRecoveryHours, Is.EqualTo(1f));
            Assert.That(_balance.HintPrice, Is.EqualTo(TestConfig.HintPrice));
            Assert.That(_balance.PriceOf(Cities), Is.EqualTo(TestConfig.CitiesPrice));
        }

        /// <summary>JSON в формате панели Remote Config читается.</summary>
        [Test]
        public void TheDashboardJson_IsRead()
        {
            _balance.Apply(BalanceOverrides.FromJson(
                "{ \"hintPrice\": 4, \"energyRecoveryHours\": 0.5, \"themePrices\": { \"cities\": 25 } }"));

            Assert.That(_balance.HintPrice, Is.EqualTo(4));
            Assert.That(_balance.EnergyRecoveryHours, Is.EqualTo(0.5f));
            Assert.That(_balance.PriceOf(Cities), Is.EqualTo(25));
        }

        /// <summary>
        /// Новое сохранение получает стартовые звёзды и энергию из Remote Config, хотя объект сохранения создан
        /// раньше, чем пришёл баланс: значения берутся при загрузке, как на экране загрузки игры.
        /// </summary>
        [Test]
        public void ANewSave_StartsWithTheOverriddenBalance()
        {
            var save = new PlayerSave(new MemoryStorage(), _config, _balance);
            _balance.Apply(new BalanceOverrides { InitialStars = 50, InitialEnergy = 3 });

            save.Load();

            Assert.That(save.GetPlayerData().Stars, Is.EqualTo(50));
            Assert.That(save.GetPlayerData().Energy.CurrentEnergy, Is.EqualTo(3));
        }
    }
}
