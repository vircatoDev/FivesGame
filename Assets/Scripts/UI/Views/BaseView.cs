using Cysharp.Threading.Tasks;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Scripts.UI.Views
{
    public abstract class BaseView : MonoBehaviour
    {
        public abstract void Initialize(BasePresenter presenter);
        public abstract UniTask PlayShowAnimation();
        public abstract UniTask PlayHideAnimation();
    }

    public abstract class View<TPresenter> : BaseView where TPresenter : BasePresenter
    {
        protected TPresenter Presenter { get; private set; }

        /// <summary>The view wires itself first, then the presenter fills it.</summary>
        public sealed override void Initialize(BasePresenter presenter)
        {
            Presenter = (TPresenter)presenter;
            OnInitialized();
            Presenter.Initialize(this);
        }

        protected virtual void OnInitialized()
        {
        }
    }
}
