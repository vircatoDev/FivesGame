using Scripts.Configs;
using Scripts.UI.Presenters;

namespace Scripts.Components
{
    public struct OpenScreenEvent
    {
        public StateConfig Config;
        public BasePresenter Presenter;
    }
}
