using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;

namespace Scripts.Systems
{
    public class SoundSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsFilter<PlaySoundEffectEvent> _soundEventFilter;
        private readonly SoundService _soundService;

        public SoundSystem(SoundService soundService)
        {
            _soundService = soundService;
        }
        public void Init()
        {
            _soundService.PlayBackgroundMusic(AudioKeyCollection.Background);
        }
        public void Run()
        {
            foreach (var i in _soundEventFilter)
            {
                var audioEffectName = _soundEventFilter.Get1(i).Key;
                var audioEffectVolume = _soundEventFilter.Get1(i).Volume;

                _soundService.PlaySoundEffect(audioEffectName, audioEffectVolume > 0 ? audioEffectVolume : 1f);
            }
        }
    }
}