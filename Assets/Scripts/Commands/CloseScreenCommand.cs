using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Commands
{
    public class CloseScreenCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly string _prefabName;
        private readonly bool _isPopup;

        public CloseScreenCommand(EcsWorld world, string prefabName, bool isPopup)
        {
            _world = world;
            _prefabName = prefabName;
            _isPopup = isPopup;
        }

        public void Execute()
        {
            var closeScreenEvent = _world.NewEntity();
            closeScreenEvent.Replace(new CloseScreenEvent
            {
                PrefabName = _prefabName,
                IsPopup = _isPopup
            });
        }
    }
}