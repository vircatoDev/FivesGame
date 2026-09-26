using System;
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
        [SerializeField] private LanguageOption[] languages;
        [Tooltip("Frame moved onto the selected flag.")]
        [SerializeField] private RectTransform languageMark;

        [Serializable]
        private struct LanguageOption
        {
            public string Code;
            public Button Button;
        }

        protected override void OnInitialized()
        {
            closeButton.onClick.AddListener(Presenter.OnClose);

            musicVolumeSlider.onValueChanged.AddListener(Presenter.OnChangeMusicVolume);
            soundEffectVolumeSlider.onValueChanged.AddListener(Presenter.OnChangeSFXVolume);

            musicToggle.Initialize(Presenter.GetMusicState(), Presenter.OnToggleMusic);
            soundEffectToggle.Initialize(Presenter.GetSoundEffectState(), Presenter.OnToggleSFX);

            foreach (var language in languages)
            {
                var code = language.Code;
                language.Button.onClick.AddListener(() => Presenter.OnSelectLanguage(code));
            }
        }

        public void ShowLanguage(string code)
        {
            foreach (var language in languages)
            {
                if (language.Code != code)
                    continue;

                languageMark.SetParent(language.Button.transform, false);
                languageMark.anchoredPosition = Vector2.zero;
                languageMark.localScale = Vector3.one * 0.8f;
                languageMark.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetLink(languageMark.gameObject);
            }
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

        public override UniTask PlayShowAnimation()
        {
            canvasGroup.alpha = 0;
            return Play(canvasGroup.DOFade(1, 0.5f));
        }

        public override UniTask PlayHideAnimation() => Play(canvasGroup.DOFade(0, 0.5f));
    }
}