using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;
using UnityEngine;

namespace Scripts.Services
{
    public class SoundService : ISoundService, IStorable
    {
        private readonly List<GameSoundCollection> _audioClipsCollection;

        private AudioSource _backgroundMusicGo;
        private SoundSettingsData _soundSettings;

        public SoundService(GlobalConfig gameSettings, PlayerDataSaveHelper saveHelper)
        {
            _soundSettings = saveHelper.GetPlayerData().SoundSettings ?? new SoundSettingsData();
            _audioClipsCollection = gameSettings.AudioClipsCollection;

            SetMusicVolume(_soundSettings.MusicVolume);
            SetSoundEffectVolume(_soundSettings.SoundEffectsVolume);
        }

        public void PlaySoundEffect(string key, float volume = 1f)
        {
            var clip = _audioClipsCollection.FirstOrDefault(x => x.Key == key)?.AudioClip;
            if (clip == null)
                return;

            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }

        public void PlayBackgroundMusic(string key)
        {
            var clip = _audioClipsCollection.FirstOrDefault(x => x.Key == key)?.AudioClip;
            if (clip == null)
                return;

            // Create audio source if for the first time
            if (_backgroundMusicGo == null)
            {
                var go = new GameObject("Music");
                _backgroundMusicGo = go.AddComponent<AudioSource>();
                _backgroundMusicGo.loop = true;
                _backgroundMusicGo.volume = _soundSettings.MusicVolume;
                _backgroundMusicGo.clip = clip;
            }
            else
            {
                _backgroundMusicGo.clip = clip;
            }

            _backgroundMusicGo.Play();
            _backgroundMusicGo.DOFade(_soundSettings.MusicVolume, 0.5f);
        }


        public void TurnOnBackgroundMusic()
        {
            if (_backgroundMusicGo == null)
                return;

            _backgroundMusicGo.Play();
            _backgroundMusicGo.DOFade(_soundSettings.MusicVolume, 0.5f);
        }

        public void TurnOffBackgrounMusic()
        {
            if (_backgroundMusicGo == null)
                return;

            _backgroundMusicGo
                .DOFade(0f, 0.5f)
                .OnComplete(() =>
                    _backgroundMusicGo.Stop());
        }

        public void SetMusicVolume(float volume)
        {
            _soundSettings.MusicVolume = volume;
            if (_backgroundMusicGo != null)
                _backgroundMusicGo.volume = volume;
        }

        public void SetDataFromSave(SoundSettingsData data) => _soundSettings = data;

        public void UpdatePlayerData(GameSaveData playerData) => playerData.SoundSettings = _soundSettings;

        public void SetSoundEffectVolume(float volume) => _soundSettings.SoundEffectsVolume = volume;

        public float GetMusicVolume() => _soundSettings.MusicVolume;

        public float GetSoundEffectVolume() => _soundSettings.SoundEffectsVolume;
    }

    [System.Serializable]
    public class SoundSettingsData
    {
        [SerializeField] public float MusicVolume = 1f;
        [SerializeField] public float SoundEffectsVolume = 1f;
    }
}