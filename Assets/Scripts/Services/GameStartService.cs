using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Services
{
    /// <summary>
    /// Starts a run in two steps. <see cref="Prepare"/> checks and loads, may take seconds, may fail or be cancelled, and
    /// changes nothing that would need undoing. <see cref="Begin"/> commits in one frame: energy, the board request and
    /// the switch to the game. A menu prepares, plays its hide animation and begins.
    /// </summary>
    public class GameStartService
    {
        private const int RunPrice = 1;

        private readonly GameSession _session;
        private readonly EnergyService _energy;
        private readonly EcsWorld _world;
        private readonly ISpriteLoader _sprites;
        private readonly EcsFilter<StartRunRequest> _requests;
        private readonly EcsFilter<BoardComponent> _boards;
        private bool _loading;
        private bool _prepared;
        // The screen that prepared the run: once it is gone, the preparation no longer holds other starts back.
        private CancellationToken _preparedFor;

        public GameStartService(GameSession session, EnergyService energy, EcsWorld world, ISpriteLoader sprites)
        {
            _sprites = sprites;
            _session = session;
            _energy = energy;
            _world = world;
            _requests = (EcsFilter<StartRunRequest>)world.GetFilter(typeof(EcsFilter<StartRunRequest>));
            _boards = (EcsFilter<BoardComponent>)world.GetFilter(typeof(EcsFilter<BoardComponent>));
        }

        private bool IsPrepared => _prepared && !_preparedFor.IsCancellationRequested;

        private bool IsBusy => _loading || IsPrepared || !_requests.IsEmpty() || !_boards.IsEmpty();

        /// <summary>
        /// Checks the energy and loads the puzzle picture, which the board and the screen read synchronously. False when
        /// refused: not enough energy, or another start is loading, prepared or playing. Throws OperationCanceledException
        /// when <paramref name="lifetime"/> is cancelled.
        /// </summary>
        public async UniTask<bool> Prepare(ThemeConfig theme, PuzzleData puzzle, CancellationToken lifetime)
        {
            if (IsBusy || theme == null || puzzle == null)
                return false;

            if (_energy.GetBalance() < RunPrice)
            {
                _world.PlaySound(AudioKeyCollection.WrongClick);
                _world.Send(CurrencyChangedEvent.NotEnough(Currency.Energy));
                return false;
            }

            _loading = true;
            try
            {
                _sprites.Release(this); // the previous run's picture
                var image = await _sprites.Load(puzzle.Image, this).AttachExternalCancellation(lifetime);
                _session.SetSelectedTheme(theme);
                _session.SetSelectedImage(puzzle, image);
            }
            finally
            {
                _loading = false;
            }

            _prepared = true;
            _preparedFor = lifetime;
            return true;
        }

        /// <summary>Spends the energy, asks the world for the board and switches to the game, all in this frame.</summary>
        public void Begin()
        {
            if (!IsPrepared)
                throw new InvalidOperationException("Begin needs a successful Prepare from a screen that is still open.");
            // Only a start spends energy and one start is prepared at a time, so the checked balance is still there.
            if (!_energy.Spend(RunPrice))
                throw new InvalidOperationException("The energy checked in Prepare was spent before Begin.");

            _prepared = false;
            _session.BeginRun();
            _world.NewEntity().Get<StartRunRequest>();
            _world.ChangeState(GameStateType.Playing);
        }
    }
}
