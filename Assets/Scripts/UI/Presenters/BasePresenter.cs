using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public abstract class BasePresenter
    {
        public abstract void Initialize(BaseView initData);
        public abstract void OnActivateView();
    }
}