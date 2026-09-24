using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Services
{
    public enum PurchaseResult { Unlocked, AlreadyUnlocked, NotEnoughStars }

    /// <summary>Unlocks a theme for stars: debit, unlock, header update and saves. Presenters only show the result.</summary>
    public sealed class ThemeShop
    {
        private readonly StarService _stars;
        private readonly PlayerProgressService _progress;
        private readonly EcsWorld _world;

        public ThemeShop(StarService stars, PlayerProgressService progress, EcsWorld world)
        {
            _stars = stars;
            _progress = progress;
            _world = world;
        }

        public PurchaseResult TryUnlock(ThemeConfig theme)
        {
            if (_progress.IsUnlocked(theme))
                return PurchaseResult.AlreadyUnlocked;

            if (theme.UnlockCost < 0 || (theme.UnlockCost > 0 && !_stars.Spend(theme.UnlockCost)))
            {
                _world.Send(CurrencyChangedEvent.NotEnough(Currency.Stars));
                return PurchaseResult.NotEnoughStars;
            }

            _progress.Unlock(theme);
            _world.Send(CurrencyChangedEvent.Changed(Currency.Stars, _stars.GetBalance(), -theme.UnlockCost));
            _world.Send<SaveDataEvent>();
            return PurchaseResult.Unlocked;
        }
    }
}
