using Fives.Components;
using Fives.Models;
using Fives.Services;
using Leopotam.Ecs;

namespace Fives.Systems
{
    public class SoundSystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly EcsFilter<PlaySoundEffectEvent> _sounds = null;
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
            foreach (var i in _sounds)
                _soundService.PlaySoundEffect(_sounds.Get1(i).Key);
        }
    }
}