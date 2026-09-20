using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Commands
{
    public class CloseAllScreensCommand : ICommand
    {
        private readonly EcsWorld _world;
        private string _prefabName;

        public CloseAllScreensCommand(EcsWorld world, string prefabName)
        {
            _world = world;
            _prefabName = prefabName;
        }

        public void Execute()
        {
            var closeScreenEvent = _world.NewEntity();
            closeScreenEvent.Replace(new CloseAllScreensEvent
            {
                NextScreenPrefabName = _prefabName,
            });
        }
    }
}