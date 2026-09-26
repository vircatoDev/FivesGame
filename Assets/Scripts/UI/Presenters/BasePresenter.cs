using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public abstract class BasePresenter
    {
        public abstract void Initialize(IView view);
        public abstract void OnActivateView();

        /// <summary>The screen was destroyed: the presenter releases what it loaded and lets go of the view.</summary>
        public abstract void Deactivate();

        /// <summary>Releases what the screen loaded; the view is still set.</summary>
        protected virtual void OnDeactivateView()
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

        public sealed override void Deactivate()
        {
            OnDeactivateView();
            View = null;
        }
    }
}
