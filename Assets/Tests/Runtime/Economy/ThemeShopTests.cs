using Fives.Components;
using Fives.Services;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Economy
{
    /// <summary>Покупка закрытой темы за звёзды.</summary>
    public sealed class ThemeShopTests
    {
        /// <summary>Двойной тап по покупке темы: звёзды списываются один раз.</summary>
        [Test]
        public void ThemePurchase_DebitsOnce_EvenWhenTappedTwice()
        {
            using var game = new GameStand();
            var select = game.SelectMenu(new FakeSelectMenuView());

            select.OnThemeBuy("cities");
            select.OnThemeBuy("cities");

            Assert.That(game.Stars.GetBalance(), Is.EqualTo(TestConfig.Stars - TestConfig.CitiesPrice));
        }

        /// <summary>
        /// Не хватает звёзд:
        /// тема остаётся закрытой, звёзды целы, шапка показывает нехватку, сохранения нет.
        /// </summary>
        [Test]
        public void WithoutEnoughStars_TheThemeStaysLocked_AndTheHeaderIsSignalled()
        {
            const int stars = 10; // тема cities стоит 60
            using var game = new GameStand(stars);
            var cities = game.Theme("cities");

            var result = game.Shop.TryUnlock(cities);

            Assert.That(result, Is.EqualTo(PurchaseResult.NotEnoughStars));
            Assert.That(game.Progress.IsUnlocked(cities), Is.False);
            Assert.That(game.Stars.GetBalance(), Is.EqualTo(stars));
            Assert.That(game.World.Count<CurrencyChangedEvent>(), Is.EqualTo(1));
            Assert.That(game.World.Count<SaveDataEvent>(), Is.Zero);
        }
    }
}
