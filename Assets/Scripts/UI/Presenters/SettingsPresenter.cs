using Cysharp.Threading.Tasks;
﻿using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class SettingsPresenter : Presenter<SettingsView>
    {
        private readonly SoundService _soundService;
        private readonly LanguageService _language;
        private readonly ITexts _texts;
        private bool _languageChanged;
        private readonly EcsWorld _world;

        public SettingsPresenter(SoundService soundService, LanguageService language, ITexts texts, EcsWorld world)
        {
            _texts = texts;
            _soundService = soundService;
            _language = language;
            _world = world;
        }

        public override void OnActivateView()
        {
            View.UpdateUI(_soundService.GetMusicVolume(), _soundService.GetSoundEffectVolume());
            View.ShowLanguage(_language.Current);
        }
        public bool GetMusicState() => _soundService.GetMusicVolume() > 0;
        public bool GetSoundEffectState() => _soundService.GetSoundEffectVolume() > 0;

        public void OnChangeMusicVolume(float volume)
        {
            _soundService.SetMusicVolume(volume);
            View.SetMusicToggle(volume > 0);
        }

        public void OnChangeSFXVolume(float volume)
        {
            _soundService.SetSoundEffectVolume(volume);
            View.SetSFXToggle(volume > 0);
        }

        public void OnToggleMusic(bool isEnabled)
        {
            float newVolume = isEnabled ? 1f : 0f;
            _soundService.SetMusicVolume(newVolume);
            View.SetMusicVolumeSlider(newVolume);
        }

        public void OnToggleSFX(bool isEnabled)
        {
            float newVolume = isEnabled ? 1f : 0f;
            _soundService.SetSoundEffectVolume(newVolume);
            View.SetSFXVolumeSlider(newVolume);
        }

        public void OnSelectLanguage(string code)
        {
            if (!_language.TrySet(code))
                return;

            _languageChanged = true;
            View.ShowLanguage(code);
            _world.PlaySound(AudioKeyCollection.MenuClick);
        }

        public void OnClose() => CloseAsync().Forget();

        // The main menu stays open under this popup. After a language change it is rebuilt, once the new
        // tables are loaded, so the texts it builds in code are read again.
        private async UniTaskVoid CloseAsync()
        {
            _world.Send<SaveDataEvent>();
            await _texts.Ready();
            if (_languageChanged)
                _world.Send(new CloseScreenEvent { State = GameStateType.MainMenu });
            _languageChanged = false;
            _world.ChangeState(GameStateType.MainMenu);
        }
    }
}