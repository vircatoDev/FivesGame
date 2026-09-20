using Leopotam.Ecs;
using Scripts.Components;
using Scripts.UI.Presenters;

namespace Scripts.Commands
{
    public class OpenScreenCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly string _prefabName;
        private readonly bool _isPopup;
        private readonly BasePresenter _initData;

        public OpenScreenCommand(EcsWorld world, string prefabName, bool isPopup, BasePresenter initData)
        {
            _world = world;
            _prefabName = prefabName;
            _isPopup = isPopup;
            _initData = initData;
        }

        public void Execute()
        {
            var openScreenEvent = _world.NewEntity();
            openScreenEvent.Replace(new OpenScreenEvent
            {
                PrefabName = _prefabName,
                IsPopup = _isPopup,
                InitData = _initData
            });
        }
    }
}