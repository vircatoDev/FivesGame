using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Systems
{
    /// <summary>Replaces the tiles with the whole picture when the board is solved. Runs after WinCheckSystem.</summary>
    class BoardRevealSystem : IEcsRunSystem
    {
        private readonly EcsFilter<BoardSolvedEvent> _solved = null;
        private readonly EcsFilter<BoardViewComponent> _views = null;

        public void Run()
        {
            if (_solved.GetEntitiesCount() == 0)
                return;

            foreach (var i in _views)
                _views.Get1(i).View.Reveal();
        }
    }
}
