using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;

namespace Scripts.Systems
{
    /// <summary>
    /// Brings wallet changes into the world: compares the balances with the last frame and reports a difference to the
    /// header and the save. Whoever spends or credits only calls the service, in ECS or in a menu. Several changes of one
    /// currency in a frame arrive as one event with their total delta.
    /// Runs after the systems that spend and before the header and the save, so a change is shown in the same frame.
    /// </summary>
    public sealed class CurrencySyncSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly StarService _stars;
        private readonly EnergyService _energy;
        private int _shownStars;
        private int _shownEnergy;

        // The balances are taken before any Init, so energy recovered offline at startup is reported and saved too.
        public CurrencySyncSystem(StarService stars, EnergyService energy)
        {
            _stars = stars;
            _energy = energy;
            _shownStars = stars.GetBalance();
            _shownEnergy = energy.GetBalance();
        }

        public void Run()
        {
            var changed = Sync(Currency.Stars, _stars.GetBalance(), ref _shownStars)
                          | Sync(Currency.Energy, _energy.GetBalance(), ref _shownEnergy);
            if (changed)
                _world.Send<SaveDataEvent>();
        }

        private bool Sync(Currency currency, int balance, ref int shown)
        {
            if (balance == shown)
                return false;

            _world.Send(CurrencyChangedEvent.Changed(currency, balance, balance - shown));
            shown = balance;
            return true;
        }
    }
}
