using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scripts.UI.Presenters;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class SettingsView : View<SettingsPresenter>
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundEffectVolumeSlider;
        [SerializeField] private ToggleButtonComponent musicToggle;
        [SerializeField] private ToggleButtonComponent soundEffectToggle;

        protected override void OnInitialized()
        {
            closeButton.onClick.AddListener(Presenter.OnClose);

            musicVolumeSlider.onValueChanged.AddListener(Presenter.OnChangeMusicVolume);
            soundEffectVolumeSlider.onValueChanged.AddListener(Presenter.OnChangeSFXVolume);

            musicToggle.Initialize(Presenter.GetMusicState(), Presenter.OnToggleMusic);
            soundEffectToggle.Initialize(Presenter.GetSoundEffectState(), Presenter.OnToggleSFX);
        }

        public void UpdateUI(float musicVolume, float sfxVolume)
        {
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            soundEffectVolumeSlider.SetValueWithoutNotify(sfxVolume);
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