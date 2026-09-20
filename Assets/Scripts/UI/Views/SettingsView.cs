using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts.UI.Presenters;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class SettingsView : BaseView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundEffectVolumeSlider;
        [SerializeField] private ToggleButtonComponent musicToggle;
        [SerializeField] private ToggleButtonComponent soundEffectToggle;

        private SettingsPresenter _presenter;

        public override void Initialize(BasePresenter initData)
        {
            if (initData is SettingsPresenter presenter)
            {
                _presenter = presenter;
                _presenter.Initialize(this);

                closeButton.onClick.AddListener(_presenter.OnClose);
            
                musicVolumeSlider.onValueChanged.AddListener(_presenter.OnChangeMusicVolume);
                soundEffectVolumeSlider.onValueChanged.AddListener(_presenter.OnChangeSFXVolume);
            
                musicToggle.Initialize(_presenter.GetMusicState(), _presenter.OnToggleMusic);
                soundEffectToggle.Initialize(_presenter.GetSoundEffectState(), _presenter.OnToggleSFX);
            }
            else
            {
                Debug.LogError("SettingsView: wrong initData");
            }
        }

        public void UpdateUI(float musicVolume, float sfxVolume)
        {
            musicVolumeSlider.value = musicVolume;
            soundEffectVolumeSlider.value = sfxVolume;
        }
    
        public void SetMusicVolumeSlider(float value) {
            musicVolumeSlider.value = value;
        }

        public void SetSFXVolumeSlider(float value) {
            soundEffectVolumeSlider.value = value;
        }

        public void SetMusicToggle(bool isEnabled) {
            musicToggle.SetState(isEnabled);
        }

        public void SetSFXToggle(bool isEnabled) {
            soundEffectToggle.SetState(isEnabled);
        }

        public override async UniTask PlayShowAnimation()
        {
            canvasGroup.alpha = 0;
            await canvasGroup.DOFade(1, 0.5f).AsyncWaitForCompletion();
        }

        public override async UniTask PlayHideAnimation()
        {
            await canvasGroup.DOFade(0, 0.5f).AsyncWaitForCompletion();
        }
    }
}