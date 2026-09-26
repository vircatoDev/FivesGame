using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Scripts.UI.Views
{
    public abstract class BaseView : MonoBehaviour, IView
    {
        public CancellationToken Lifetime => destroyCancellationToken;

        public abstract void Initialize(BasePresenter presenter);
        public abstract UniTask PlayShowAnimation();
        public abstract UniTask PlayHideAnimation();

        /// <summary>
        /// Awaits a screen tween. If the screen is destroyed meanwhile, the tween is killed and the await is cancelled,
        /// so whatever a presenter meant to do after the animation does not run.
        /// </summary>
        protected UniTask Play(Tween tween) =>
            tween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, destroyCancellationToken);
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

        protected virtual void OnDestroy() => Presenter?.Deactivate();
    }
}
