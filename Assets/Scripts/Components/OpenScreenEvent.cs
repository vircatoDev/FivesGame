using Scripts.UI.Presenters;

namespace Scripts.Components
{
    public struct OpenScreenEvent {
        public string PrefabName;
        public bool IsPopup;
        public BasePresenter InitData;
    }
}