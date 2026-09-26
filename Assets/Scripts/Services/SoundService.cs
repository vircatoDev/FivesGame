using System.Collections.Generic;
using DG.Tweening;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;
using UnityEngine;

namespace Scripts.Services
{
    public class SoundService : IStorable
    {
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        private AudioSource _backgroundMusicGo;
        // One source plays every effect: PlayOneShot mixes overlapping clips, where PlayClipAtPoint made an object per sound.
        private AudioSource _effects;
        private SoundSettingsData _soundSettings;

        public SoundService(GlobalConfig gameSettings, PlayerDataSaveHelper saveHelper)
        {
            _soundSettings = saveHelper.GetPlayerData().SoundSettings ?? new SoundSettingsData();
            foreach (var sound in gameSettings.AudioClipsCollection)
                _clips.TryAdd(sound.Key, sound.AudioClip);
        }

        public void PlaySoundEffect(string key, float volume = 1f)
        {
            var effectVolume = EffectVolume(volume);
            if (effectVolume <= 0f || !TryGetClip(key, out var clip))
                return;

            if (_effects == null) // first effect, or the scene that held the source was unloaded
            {
                _effects = new GameObject("SoundEffects").AddComponent<AudioSource>();
                _effects.playOnAwake = false;
            }

            _effects.PlayOneShot(clip, effectVolume);
        }

        public void PlayBackgroundMusic(string key)
        {
            if (!TryGetClip(key, out var clip))
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


        public void SetMusicVolume(float volume)
        {
            _soundSettings.MusicVolume = volume;
            if (_backgroundMusicGo != null)
                _backgroundMusicGo.volume = volume;
        }

        private bool TryGetClip(string key, out AudioClip clip) => _clips.TryGetValue(key, out clip) && clip != null;

        /// <summary>A sound effect's own volume scaled by the player's effects setting.</summary>
        public float EffectVolume(float volume) => volume * _soundSettings.SoundEffectsVolume;

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