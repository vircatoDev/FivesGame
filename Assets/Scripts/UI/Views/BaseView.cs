using Cysharp.Threading.Tasks;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Scripts.UI.Views
{
    public abstract class BaseView : MonoBehaviour {
        public abstract void Initialize(BasePresenter initData);
        public abstract UniTask PlayShowAnimation();
        public abstract UniTask PlayHideAnimation();
    }
}