using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Services
{
    public class GameStartService
    {
        private readonly GameSession _session;
        private readonly EnergyService _energy;
        private readonly EcsWorld _world;

        public GameStartService(GameSession session, EnergyService energy, EcsWorld world)
        {
            _session = session;
            _energy = energy;
            _world = world;
        }

        public bool TryStart(ThemeConfig theme, PuzzleData puzzle)
        {
            if (_session.IsRunning || theme == null || puzzle == null)
            {
                return false;
            }

            if (!_energy.Spend(1))
            {
                _world.PlaySound(AudioKeyCollection.WrongClick);
                _world.Send(CurrencyChangedEvent.NotEnough(Currency.Energy));
                return false;
            }

            _session.SetSelectedTheme(theme);
            _session.SetSelectedImage(puzzle);
            _session.BeginRun();
            _world.Send(CurrencyChangedEvent.Changed(Currency.Energy, _energy.GetBalance(), -1));
            _world.Send(new SaveDataEvent { StorableObject = _energy });
            return true;
        }
    }
}
