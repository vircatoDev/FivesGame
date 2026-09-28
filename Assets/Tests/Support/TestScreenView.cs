using Cysharp.Threading.Tasks;
using Fives.UI.Presenters;
using Fives.UI.Views;

namespace Fives.Tests.Support
{
    /// <summary>
    /// A screen without a prefab, for ScreenNavigator tests. It lives in this runtime assembly, compiled only with the
    /// test framework, because Unity does not attach MonoBehaviours from editor-only test assemblies.
    /// </summary>
    public sealed class TestScreenView : View<TestScreenPresenter>
    {
        public TestScreenPresenter Current => Presenter;
        public override UniTask PlayShowAnimation() => UniTask.CompletedTask;
        public override UniTask PlayHideAnimation() => UniTask.CompletedTask;
    }

    public sealed class TestScreenPresenter : Presenter<IView>
    {
        public int Activations;
        public override void OnActivateView() => Activations++;
    }
}
