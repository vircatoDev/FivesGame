using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;

namespace Scripts.Systems
{
    /// <summary>
    /// Records the solved puzzle as completed and saves it in the frame it is solved, whatever the player does next:
    /// leaving during the pause before the result screen, or the OS closing the app, keeps the completion.
    /// </summary>
    sealed class PuzzleCompletionSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly GameSession _session = null;
        private readonly EcsFilter<BoardSolvedEvent> _solved = null;
        private readonly PlayerProgressService _progress;

        public PuzzleCompletionSystem(PlayerProgressService progress)
        {
            _progress = progress;
        }

        public void Run()
        {
            if (_solved.IsEmpty())
                return;

            _progress.MarkCompleted(_session.SelectedPuzzle);
            _world.Send<SaveDataEvent>();
        }
    }
}
