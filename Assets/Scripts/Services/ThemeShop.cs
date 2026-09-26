using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Services
{
    public enum PurchaseResult { Unlocked, AlreadyUnlocked, NotEnoughStars }

    /// <summary>Unlocks a theme for stars and saves the unlock. Presenters only show the result.</summary>
    public sealed class ThemeShop
    {
        private readonly StarService _stars;
        private readonly PlayerProgressService _progress;
        private readonly EcsWorld _world;
        private readonly GameBalance _balance;

        public ThemeShop(StarService stars, PlayerProgressService progress, EcsWorld world, GameBalance balance)
        {
            _stars = stars;
            _progress = progress;
            _world = world;
            _balance = balance;
        }

        public int PriceOf(ThemeConfig theme) => _balance.PriceOf(theme);

        public PurchaseResult TryUnlock(ThemeConfig theme)
        {
            if (_progress.IsUnlocked(theme))
                return PurchaseResult.AlreadyUnlocked;

            if (!Pay(PriceOf(theme)))
            {
                _world.Send(CurrencyChangedEvent.NotEnough(Currency.Stars));
                return PurchaseResult.NotEnoughStars;
            }

            _progress.Unlock(theme);
            // CurrencySyncSystem shows the debit and saves it; the unlock needs a save of its own, for a free theme too.
            _world.Send<SaveDataEvent>();
            return PurchaseResult.Unlocked;
        }

        // Debits the price. A free theme costs nothing; a negative price is a config error and never unlocks.
        private bool Pay(int price) => price == 0 || (price > 0 && _stars.Spend(price));
    }
}
