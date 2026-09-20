using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class ToggleButtonComponent : MonoBehaviour
    {
        [SerializeField] private Button toggleButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite enabledSprite;
        [SerializeField] private Sprite disabledSprite;

        private bool _isEnabled;
        private UnityAction<bool> _onToggleCallback;

        public void Initialize(bool initialState, UnityAction<bool> onToggle)
        {
            _isEnabled = initialState;
            _onToggleCallback = onToggle;
            UpdateIcon();

            toggleButton.onClick.AddListener(Toggle);
        }

        private void Toggle()
        {
            SetState(!_isEnabled);
            _onToggleCallback?.Invoke(_isEnabled);
        }

        public void SetState(bool isEnabled)
        {
            _isEnabled = isEnabled;
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            iconImage.sprite = _isEnabled ? enabledSprite : disabledSprite;
        }

        private void OnDestroy()
        {
            toggleButton.onClick.RemoveListener(Toggle);
        }
    }
}