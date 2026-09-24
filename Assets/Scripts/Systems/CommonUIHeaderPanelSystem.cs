using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.Systems
{
    public class CommonUIHeaderPanelSystem : IEcsRunSystem,IEcsInitSystem
    {
        private readonly IHeaderPanelView _headerPanelView;
        private readonly EnergyService _energy;
        private readonly StarService _stars;
        private readonly EcsFilter<CurrencyChangedEvent> _currencyFilter;
        private readonly EcsFilter<UpdateControlPanelBtnLogicEvent> _updateBtnLogicFilter;


        public CommonUIHeaderPanelSystem(IHeaderPanelView headerPanelView, EnergyService energy, StarService stars)
        {
            _headerPanelView = headerPanelView;
            _energy = energy;
            _stars = stars;
        }
        public void Init()
        {
            _headerPanelView.UpdateViewContent(_stars.GetBalance().ToString(), _energy.GetBalance().ToString());
        }
        public void Run()
        {
            foreach (var i in _currencyFilter)
                _headerPanelView.UpdateCurrency(_currencyFilter.Get1(i));

            foreach (var i in _updateBtnLogicFilter)
            {
                ref var updateControlPanelEventEvent = ref _updateBtnLogicFilter.Get1(i);
                _headerPanelView.UpdateButtonLogic(updateControlPanelEventEvent);
            }
        }
    
    }
}