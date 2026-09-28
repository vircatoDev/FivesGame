using Fives.Components;
using Fives.Services;
using Fives.UI.Views;
using Leopotam.Ecs;

namespace Fives.Systems
{
    public class CommonUIHeaderPanelSystem : IEcsRunSystem,IEcsInitSystem
    {
        private readonly IHeaderPanelView _headerPanelView;
        private readonly EnergyService _energy;
        private readonly StarService _stars;
        private readonly EcsFilter<CurrencyChangedEvent> _currencyFilter;


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
        }
    
    }
}