using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fives.UI.Presenters;
using UnityEngine;

namespace Fives.UI.Views
{
    public abstract class BaseView : MonoBehaviour, IView
    {
        public CancellationToken Lifetime => destroyCancellationToken;

        /// <summary>The presenter this view works with; ScreenNavigator resolves it from the container.</summary>
        public abstract Type PresenterType { get; }

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

        public sealed override Type PresenterType => typeof(TPresenter);

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
