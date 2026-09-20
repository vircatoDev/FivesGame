using Scripts.Commands;
using Scripts.Helpers.StateMachine;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class SettingsPresenter : BasePresenter
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SoundService _soundService;
        private readonly ECSCommandService _ecsCommandService;
        private SettingsView _view;

        public SettingsPresenter(SoundService soundService, ECSCommandService ecsCommandService)
        {
            _soundService = soundService;
            _ecsCommandService = ecsCommandService;
        }

        public override void Initialize(BaseView initData)
        {
            _view = initData as SettingsView;
            OnActivateView();
        }

        public override void OnActivateView() => _view.UpdateUI(_soundService.GetMusicVolume(), _soundService.GetSoundEffectVolume());
        public bool GetMusicState() => _soundService.GetMusicVolume() > 0;
        public bool GetSoundEffectState() => _soundService.GetSoundEffectVolume() > 0;

        public void OnChangeMusicVolume(float volume)
        {
            _soundService.SetMusicVolume(volume);
            _view.SetMusicToggle(volume > 0);
        }

        public void OnChangeSFXVolume(float volume)
        {
            _soundService.SetSoundEffectVolume(volume);
            _view.SetSFXToggle(volume > 0);
        }

        public void OnToggleMusic(bool isEnabled)
        {
            float newVolume = isEnabled ? 1f : 0f;
            _soundService.SetMusicVolume(newVolume);
            _view.SetMusicVolumeSlider(newVolume);
        }

        public void OnToggleSFX(bool isEnabled)
        {
            float newVolume = isEnabled ? 1f : 0f;
            _soundService.SetSoundEffectVolume(newVolume);
            _view.SetSFXVolumeSlider(newVolume);
        }

        public void OnClose()
        {
            _ecsCommandService.CreateCommand<SaveDataCommand>(_soundService).Execute();
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.MainMenu).Execute();
        }
    }
}