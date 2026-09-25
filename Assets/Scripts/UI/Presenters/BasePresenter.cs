using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public abstract class BasePresenter
    {
        public abstract void Initialize(IView view);
        public abstract void OnActivateView();

        /// <summary>The screen was destroyed: release what it loaded.</summary>
        public virtual void OnDeactivateView()
        {
        }
    }

    public abstract class Presenter<TView> : BasePresenter where TView : class, IView
    {
        protected TView View { get; private set; }

        public sealed override void Initialize(IView view)
        {
            View = (TView)view;
            OnActivateView();
        }
    }
}
