using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Helpers;
using Scripts.UI.Views;

namespace Scripts.Systems
{
    public class CommonUIHeaderPanelSystem : IEcsRunSystem,IEcsInitSystem
    {
        private readonly HeaderPanelView _headerPanelView;
        private readonly PlayerDataSaveHelper _playerDataSaveHelper;
        private readonly EcsFilter<UpdateControlPanelEnergyEvent> _updateEnergyFilter;
        private readonly EcsFilter<UpdateControlPanelStarsEvent> _updateStarsFilter;
        private readonly EcsFilter<UpdateControlPanelBtnLogicEvent> _updateBtnLogicFilter;


        public CommonUIHeaderPanelSystem(HeaderPanelView headerPanelView)
        {
            _headerPanelView = headerPanelView;
        }
        public void Init()
        {
            var gameSaveData = _playerDataSaveHelper.GetPlayerData();
            _headerPanelView.UpdateViewContent(gameSaveData.Stars.ToString(), gameSaveData.Energy.CurrentEnergy.ToString());
        }
        public void Run()
        {
            foreach (var i in _updateEnergyFilter)
            {
                ref var updateControlPanelEventEvent = ref _updateEnergyFilter.Get1(i);
                _headerPanelView.UpdateEnergy(updateControlPanelEventEvent);
            }

            foreach (var i in _updateStarsFilter)
            {
                ref var updateControlPanelEventEvent = ref _updateStarsFilter.Get1(i);
                _headerPanelView.UpdateStars(updateControlPanelEventEvent);
            }

            foreach (var i in _updateBtnLogicFilter)
            {
                ref var updateControlPanelEventEvent = ref _updateBtnLogicFilter.Get1(i);
                _headerPanelView.UpdateButtonLogic(updateControlPanelEventEvent);
            }
        }
    
    }
}