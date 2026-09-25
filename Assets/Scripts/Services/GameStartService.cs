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
        private readonly EcsFilter<StartRunRequest> _requests;
        private readonly EcsFilter<BoardComponent> _boards;
        private bool _loading;

        public GameStartService(GameSession session, EnergyService energy, EcsWorld world, ISpriteLoader sprites)
        {
            _sprites = sprites;
            _session = session;
            _energy = energy;
            _world = world;
            _requests = (EcsFilter<StartRunRequest>)world.GetFilter(typeof(EcsFilter<StartRunRequest>));
            _boards = (EcsFilter<BoardComponent>)world.GetFilter(typeof(EcsFilter<BoardComponent>));
        }

        /// <summary>
        /// Spends energy, loads the puzzle picture, which the board and the screen read synchronously, and asks the
        /// world for a board. Refused while a start is loading, waiting for the Playing state, or a board is in play.
        /// </summary>
        public async UniTask<bool> TryStart(ThemeConfig theme, PuzzleData puzzle)
        {
            if (_loading || !_requests.IsEmpty() || !_boards.IsEmpty() || theme == null || puzzle == null)
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

            _loading = true;
            try
            {
                _sprites.Release(this); // the previous run's picture
                _session.SetSelectedImage(puzzle, await _sprites.Load(puzzle.Image, this));
            }
            finally
            {
                _loading = false;
            }

            _world.NewEntity().Get<StartRunRequest>();
            return true;
        }
    }
}
