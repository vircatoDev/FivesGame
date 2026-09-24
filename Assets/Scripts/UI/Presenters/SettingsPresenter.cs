using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class SettingsPresenter : Presenter<SettingsView>
    {
        private readonly SoundService _soundService;
        private readonly EcsWorld _world;

        public SettingsPresenter(SoundService soundService, EcsWorld world)
        {
            _soundService = soundService;
            _world = world;
        }

        public override void OnActivateView() => View.UpdateUI(_soundService.GetMusicVolume(), _soundService.GetSoundEffectVolume());
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

        public void OnClose()
        {
            _world.Send<SaveDataEvent>();
            _world.ChangeState(GameStateType.MainMenu);
        }
    }
}