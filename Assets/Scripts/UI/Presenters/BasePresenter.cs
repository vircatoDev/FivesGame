using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public abstract class BasePresenter
    {
        public abstract void Initialize(BaseView view);
        public abstract void OnActivateView();
    }

    public abstract class Presenter<TView> : BasePresenter where TView : BaseView
    {
        protected TView View { get; private set; }

        public sealed override void Initialize(BaseView view)
        {
            View = (TView)view;
            OnActivateView();
        }
    }
}
