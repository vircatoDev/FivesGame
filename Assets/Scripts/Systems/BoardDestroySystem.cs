using Leopotam.Ecs;
using Scripts.Components;
using Object = UnityEngine.Object;

namespace Scripts.Systems
{
    /// <summary>Ends the run on GameEndEvent: the board entity with its view, the tiles and a start request not yet played.</summary>
    class BoardDestroySystem : IEcsRunSystem
    {
        private readonly EcsFilter<GameEndEvent> _ends = null;
        private readonly EcsFilter<BoardComponent> _boards = null;
        private readonly EcsFilter<TileComponent> _tiles = null;
        private readonly EcsFilter<StartRunRequest> _requests = null;

        public void Run()
        {
            if (_ends.IsEmpty())
                return;

            foreach (var i in _boards)
            {
                var entity = _boards.GetEntity(i);
                if (entity.Has<BoardViewComponent>())
                    Object.Destroy(entity.Get<BoardViewComponent>().View.gameObject); // the tiles are its children
                entity.Destroy();
            }

            // Tiles exist only with a board.
            foreach (var i in _tiles)
                _tiles.GetEntity(i).Destroy();

            foreach (var i in _requests)
                _requests.GetEntity(i).Destroy();
        }
    }
}
