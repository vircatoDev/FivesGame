using Cysharp.Threading.Tasks;
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
        private readonly ISpriteLoader _sprites;

        public GameStartService(GameSession session, EnergyService energy, EcsWorld world, ISpriteLoader sprites)
        {
            _sprites = sprites;
            _session = session;
            _energy = energy;
            _world = world;
        }

        /// <summary>
        /// Spends energy, starts the run and loads the puzzle picture, which the board and the screen read synchronously.
        /// The run starts before the load, so a second tap meanwhile is refused; the board appears only on the gameplay state.
        /// </summary>
        public async UniTask<bool> TryStart(ThemeConfig theme, PuzzleData puzzle)
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
            _session.BeginRun();
            _world.Send(CurrencyChangedEvent.Changed(Currency.Energy, _energy.GetBalance(), -1));
            _world.Send<SaveDataEvent>();

            _sprites.Release(this); // the previous run's picture
            _session.SetSelectedImage(puzzle, await _sprites.Load(puzzle.Image, this));
            return true;
        }
    }
}
